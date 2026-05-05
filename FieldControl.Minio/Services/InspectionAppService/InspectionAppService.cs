using FieldControl.Minio.Data;
using FieldControl.Minio.Entities;
using FieldControl.Minio.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace FieldControl.Minio.Services.InspectionAppService
{
    public class InspectionAppService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName;

        public InspectionAppService(
            AppDbContext context,
            IFileStorageService storageService,
            IConfiguration config)
        {
            _context = context;
            _storageService = storageService;
            _bucketName = config["MinioSettings:BucketName"]!;
        }

        // INSPECTION

        public async Task<Inspection> CreateAsync(Inspection inspection)
        {
            inspection.Id = Guid.NewGuid();
            inspection.CreatedAt = DateTime.UtcNow;
            inspection.UpdatedAt = DateTime.UtcNow;

            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();

            return inspection;
        }

        public async Task<List<Inspection>> GetAllAsync()
        {
            return await _context.Inspections
                .Include(i => i.InspectionFiles)
                .ToListAsync();
        }

        public async Task<Inspection?> GetByIdAsync(Guid id)
        {
            return await _context.Inspections
                .Include(i => i.InspectionFiles)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> UpdateAsync(Guid id, Inspection updated)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return false;

            inspection.ProductName = updated.ProductName;
            inspection.Description = updated.Description;
            inspection.InspectorName = updated.InspectorName;
            inspection.Status = updated.Status;
            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.InspectionFiles)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null) return false;

            foreach (var file in inspection.InspectionFiles)
            {
                await _storageService.DeleteFileAsync(file.BucketName, file.StoredFileName);
            }

            _context.InspectionFiles.RemoveRange(inspection.InspectionFiles);
            _context.Inspections.Remove(inspection);

            await _context.SaveChangesAsync();
            return true;
        }

        
        // FILE

        public async Task<bool> UploadFilesAsync(Guid inspectionId, List<IFormFile> files)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entities = new List<InspectionFile>();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);

                    var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";

                    await _storageService.UploadFileAsync(
                        _bucketName,
                        storedFileName,
                        ms.ToArray(),
                        file.ContentType
                    );

                    entities.Add(new InspectionFile
                    {
                        Id = Guid.NewGuid(),
                        InspectionId = inspectionId,
                        FileName = file.FileName,
                        StoredFileName = storedFileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        BucketName = _bucketName,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.InspectionFiles.AddRange(entities);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<InspectionFile>> GetFilesAsync(Guid inspectionId)
        {
            return await _context.InspectionFiles
                .Where(f => f.InspectionId == inspectionId)
                .ToListAsync();
        }

        public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null) return null;

            var bytes = await _storageService.DownloadFileAsync(file.BucketName, file.StoredFileName);

            return (bytes, file.ContentType, file.FileName);
        }

        public async Task<bool> DeleteFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null) return false;

            await _storageService.DeleteFileAsync(file.BucketName, file.StoredFileName);

            _context.InspectionFiles.Remove(file);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(byte[] ZipBytes, string FileName)?> ExportFilesAsZipAsync(Guid inspectionId)
        {
            var files = await _context.InspectionFiles
                .Where(f => f.InspectionId == inspectionId)
                .ToListAsync();

            if (files.Count == 0) return null;

            using var zipStream = new MemoryStream();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var file in files)
                {
                    var bytes = await _storageService.DownloadFileAsync(file.BucketName, file.StoredFileName);

                    var safeName = $"{Guid.NewGuid()}_{file.FileName}";

                    var entry = archive.CreateEntry(safeName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(bytes);
                }
            }

            return (zipStream.ToArray(), $"inspection_{inspectionId}.zip");
        }
    }
}