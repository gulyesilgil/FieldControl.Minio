using FieldControl.Minio.Data;
using FieldControl.Minio.DTOs.InspectionFile;
using FieldControl.Minio.DTOs.InspectionFile;
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

        //  UPLOAD (STREAM)
        public async Task<List<InspectionFileDto>> UploadFilesAsync(Guid inspectionId, List<IFormFile> files)
        {
            var inspection = await _context.Inspections.FindAsync(inspectionId);
            if (inspection == null) return new List<InspectionFileDto>();

            var entities = new List<InspectionFile>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();

                await _storageService.UploadFileAsync(
                    _bucketName,
                    storedFileName,
                    stream,
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

        //  GET ALL
        public async Task<List<InspectionFileDto>> GetAllFilesAsync()
        {
            var files = await _context.InspectionFiles.ToListAsync();
            return files.Select(MapToDto).ToList();
        }

        //  GET BY INSPECTION
        public async Task<List<InspectionFileDto>> GetFilesAsync(Guid inspectionId)
        {
            var files = await _context.InspectionFiles
                .Where(f => f.InspectionId == inspectionId)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
        }

        // 🔥 DOWNLOAD (STREAM)
        public async Task<(Stream Stream, string ContentType, string FileName)?>
            DownloadFileAsync(Guid inspectionId, Guid fileId)
        {
            var file = await _context.InspectionFiles
                .FirstOrDefaultAsync(x => x.Id == fileId && x.InspectionId == inspectionId);

            if (file == null) return null;

            var stream = await _storageService.DownloadFileAsync(
                file.BucketName,
                file.StoredFileName
            );

            return (stream, file.ContentType, file.FileName);
        }

        //  DELETE
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

        //  ZIP (STREAM → ZIP → RAM FINAL)
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
                    try
                    {
                        using var stream = await _storageService.DownloadFileAsync(
                            file.BucketName,
                            file.StoredFileName
                        );

                        var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);

                        using var entryStream = entry.Open();

                        await stream.CopyToAsync(entryStream);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return (zipStream.ToArray(), $"inspection_{inspectionId}.zip");
        }

        //  MAPPING (TEK NOKTA)
        private InspectionFileDto MapToDto(InspectionFile file)
        {
            return new InspectionFileDto
            {
                Id = file.Id,
                InspectionId = file.InspectionId,
                FileName = file.FileName,
                FileSize = file.FileSize,
                ContentType = file.ContentType,
                CreatedAt = file.CreatedAt
            };
        }
    }
}