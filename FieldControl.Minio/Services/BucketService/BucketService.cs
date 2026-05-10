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

        public async Task<(byte[] ZipBytes, string FileName)?> DownloadEntireBucketAsync()
        {
            //  1. Bucket içindeki tüm dosyaları al
            var objectNames = await _storageService.ListFilesAsync(_bucketName);

            if (objectNames == null || objectNames.Count == 0)
                return null;

            using var zipStream = new MemoryStream();

            //  2. ZIP oluştur
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var objectName in objectNames)
                {
                    //  3. Dosyayı indir
                    var bytes = await _storageService.DownloadFileAsync(
                        _bucketName,
                        objectName
                    );

                    //  4. ZIP entry oluştur
                    var entry = archive.CreateEntry(objectName, CompressionLevel.Optimal);

                    using var entryStream = entry.Open();
                    await entryStream.WriteAsync(bytes, 0, bytes.Length);
                }
            }

            //  5. sonucu döndür
            return (zipStream.ToArray(), "bucket-export.zip");
        }
    }
}