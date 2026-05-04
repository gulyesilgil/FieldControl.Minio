using Amazon.S3;
using Amazon.S3.Model;
using FieldControl.Minio.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FieldControl.Minio.Services
{
    public class MinioFileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public MinioFileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["MinioSettings:BucketName"];
        }

        // 📤 UPLOAD
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var storedFileName = $"{Guid.NewGuid()}_{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = storedFileName,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);

            return storedFileName;
        }

        // 📥 DOWNLOAD
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);

            return response.ResponseStream;
        }

        // 🗑 DELETE
        public async Task DeleteFileAsync(string fileName)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}