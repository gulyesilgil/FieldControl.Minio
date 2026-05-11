
namespace FieldControl.Minio.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream stream,
            string contentType);

        Task<Stream> DownloadFileAsync(
            string bucketName,
            string objectName);

        Task DeleteFileAsync(
            string bucketName,
            string objectName);

        Task<List<string>> ListFilesAsync(string bucketName);
    }
}