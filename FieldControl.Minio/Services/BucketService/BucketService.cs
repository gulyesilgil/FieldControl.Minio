using FieldControl.Minio.Interfaces;
using System.IO.Compression;

namespace FieldControl.Minio.Services.BucketService
{
    public class BucketService
    {
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName;

        public BucketService(
            IFileStorageService storageService,
            IConfiguration config)
        {
            _storageService = storageService;
            _bucketName = config["MinioSettings:BucketName"]!;
        }

        public async Task StreamBucketAsZipAsync(Stream outputStream)
        {
            var objectNames = await _storageService.ListFilesAsync(_bucketName);

            using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);

            foreach (var objectName in objectNames)
            {
                try
                {
                    using var fileStream = await _storageService.DownloadFileAsync(
                        _bucketName,
                        objectName
                    );

                    var entry = archive.CreateEntry(objectName, CompressionLevel.Fastest);

                    using var entryStream = entry.Open();

                    await fileStream.CopyToAsync(entryStream);
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}