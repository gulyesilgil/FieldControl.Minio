using FieldControl.Minio.Data;
using FieldControl.Minio.Entities;
using FieldControl.Minio.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/inspections/{inspectionId}/files")]
    public class InspectionFilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName = "fieldcontrol-bucket";

        public InspectionFilesController(
            AppDbContext context,
            IFileStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        // POST /api/inspections/{id}/files
        [HttpPost]
        public async Task<IActionResult> Upload(Guid inspectionId, List<IFormFile> files)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);

            if (inspection == null)
                return NotFound("Inspection not found");

            var uploadedFiles = new List<InspectionFile>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                // byte[]'e çevir
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // StoredFileName (KEY)
                var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";

                // MinIO'ya upload
                await _storageService.UploadFileAsync(
                    _bucketName,
                    storedFileName,
                    fileBytes,
                    file.ContentType
                );

                // DB'ye kaydet
                var inspectionFile = new InspectionFile
                {
                    Id = Guid.NewGuid(),
                    InspectionId = inspectionId,
                    FileName = file.FileName,
                    StoredFileName = storedFileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    BucketName = _bucketName,
                    CreatedAt = DateTime.UtcNow
                };

                uploadedFiles.Add(inspectionFile);
            }

            _context.InspectionFiles.AddRange(uploadedFiles);
            await _context.SaveChangesAsync();

            return Ok(uploadedFiles);
        }

        // GET /api/inspections/{id}/files
        [HttpGet]
        public async Task<IActionResult> GetFiles(Guid inspectionId)
        {
            var files = await _context.InspectionFiles
                .Where(f => f.InspectionId == inspectionId)
                .ToListAsync();

            return Ok(files);
        }

        // GET /api/inspections/{id}/files/{fileId}/download
        [HttpGet("{fileId}/download")]
        public async Task<IActionResult> Download(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null)
                return NotFound();

            var fileBytes = await _storageService.DownloadFileAsync(
                file.BucketName,
                file.StoredFileName
            );

            return File(fileBytes, file.ContentType, file.FileName);
        }

        // DELETE /api/inspections/{id}/files/{fileId}
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null)
                return NotFound();

            // MinIO'dan sil
            await _storageService.DeleteFileAsync(
                file.BucketName,
                file.StoredFileName
            );

            // DB'den sil
            _context.InspectionFiles.Remove(file);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}