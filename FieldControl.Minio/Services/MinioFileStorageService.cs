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
            _bucketName = configuration["MinioSettings:BucketName"]!;
        }

        public async Task<string> UploadFileAsync(
            string bucketName,
            string objectName,
            byte[] data,
            string contentType)
        {
            using var stream = new MemoryStream(data);

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                InputStream = stream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);

            return objectName;
        }

        public async Task<byte[]> DownloadFileAsync(
            string bucketName,
            string objectName)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectName
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var memoryStream = new MemoryStream();

            await response.ResponseStream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        public async Task DeleteFileAsync(
            string bucketName,
            string objectName)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectName
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}