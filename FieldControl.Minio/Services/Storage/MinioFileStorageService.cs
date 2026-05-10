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

        public async Task<List<string>> ListFilesAsync(string bucketName)
        {
            var result = new List<string>();

            string? continuationToken = null;

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    ContinuationToken = continuationToken
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                foreach (var obj in response.S3Objects)
                {
                    result.Add(obj.Key); //  StoredFileName
                }

                continuationToken = response.IsTruncated == true
                    ? response.NextContinuationToken
                     : null;

            } while (continuationToken != null);

            return result;
        }
    }
}