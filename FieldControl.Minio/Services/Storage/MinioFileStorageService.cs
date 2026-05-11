using Amazon.S3;
using Amazon.S3.Model;
using FieldControl.Minio.Interfaces;

namespace FieldControl.Minio.Services.Storage
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

        // 🔥 STREAM UPLOAD
        public async Task<string> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream stream,
            string contentType)
        {
            stream.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false
            };

            await _s3Client.PutObjectAsync(request);

            return objectName;
        }

        // 🔥 STREAM DOWNLOAD
        public async Task<Stream> DownloadFileAsync(
            string bucketName,
            string objectName)
        {
            var response = await _s3Client.GetObjectAsync(bucketName, objectName);

            return response.ResponseStream; // 💥 RAM YOK
        }

        public async Task DeleteFileAsync(
            string bucketName,
            string objectName)
        {
            await _s3Client.DeleteObjectAsync(bucketName, objectName);
        }

        public async Task<List<string>> ListFilesAsync(string bucketName)
        {
            var result = new List<string>();

            string? continuationToken = null;

            do
            {
                var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    ContinuationToken = continuationToken
                });

                result.AddRange(response.S3Objects.Select(x => x.Key));

                continuationToken = response.IsTruncated == true
                         ? response.NextContinuationToken
                        : null;

            } while (continuationToken != null);

            return result;
        }
    }
}