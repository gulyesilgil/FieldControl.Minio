using System.Threading.Tasks;

namespace FieldControl.Minio.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Dosyayı Object Storage'a yükler
        /// </summary>
        Task<string> UploadFileAsync(
            string bucketName,
            string objectName,
            byte[] data,
            string contentType);

        /// <summary>
        /// Dosyayı Object Storage'dan indirir
        /// </summary>
        Task<byte[]> DownloadFileAsync(
            string bucketName,
            string objectName);

        /// <summary>
        /// Dosyayı Object Storage'dan siler
        /// </summary>
        Task DeleteFileAsync(
            string bucketName,
            string objectName);

        /// <summary>
        /// Bucket içindeki tüm dosyaları listeler (StoredFileName döner)
        /// </summary>
        Task<List<string>> ListFilesAsync(string bucketName);
    }
}