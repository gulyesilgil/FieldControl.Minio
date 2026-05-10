using FieldControl.Minio.Data;
using FieldControl.Minio.DTOs.File;
using FieldControl.Minio.Entities;
using FieldControl.Minio.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

namespace FieldControl.Minio.Services.InspectionFileService
{
    public class InspectionFileService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName;

        public InspectionFileService(
            AppDbContext context,
            IFileStorageService storageService,
            IConfiguration config)
        {
            _context = context;
            _storageService = storageService;
            _bucketName = config["MinioSettings:BucketName"]!;
        }

        // UPLOAD
        public async Task<List<InspectionFileDto>> UploadFilesAsync(Guid inspectionId, List<IFormFile> files)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return new List<InspectionFileDto>();

            var entities = new List<InspectionFile>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";

                await _storageService.UploadFileAsync(
                    _bucketName,
                    storedFileName,
                    ms.ToArray(),
                    file.ContentType
                );

                var entity = new InspectionFile
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

                entities.Add(entity);
            }

            _context.InspectionFiles.AddRange(entities);
            await _context.SaveChangesAsync();

            return entities.Select(MapToDto).ToList();
        }

        // GET FILES
        public async Task<List<InspectionFileDto>> GetFilesAsync(Guid inspectionId)
        {
            var files = await _context.InspectionFiles
                .Where(f => f.InspectionId == inspectionId)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
        }

        // DOWNLOAD
        public async Task<(byte[] FileBytes, string ContentType, string FileName)?> DownloadFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null) return null;

            var bytes = await _storageService.DownloadFileAsync(
                file.BucketName,
                file.StoredFileName
            );

            return (bytes, file.ContentType, file.FileName);
        }

        // DELETE FILE
        public async Task<bool> DeleteFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(f => f.Id == fileId && f.InspectionId == inspectionId);

            if (file == null) return false;

            await _storageService.DeleteFileAsync(
                file.BucketName,
                file.StoredFileName
            );

            _context.InspectionFiles.Remove(file);
            await _context.SaveChangesAsync();

            return true;
        }

        // EXPORT ZIP
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
                    var bytes = await _storageService.DownloadFileAsync(
                        file.BucketName,
                        file.StoredFileName
                    );

                    var entry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(bytes);
                }
            }

            return (zipStream.ToArray(), $"inspection_{inspectionId}.zip");
        }

        // 🔁 MAPPING
        private InspectionFileDto MapToDto(InspectionFile file)
        {
            return new InspectionFileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                FileSize = file.FileSize,
                ContentType = file.ContentType,
                CreatedAt = file.CreatedAt
            };
        }
    }
}