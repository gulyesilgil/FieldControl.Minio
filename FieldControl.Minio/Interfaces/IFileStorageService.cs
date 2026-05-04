using System.Threading.Tasks;

namespace FieldControl.Minio.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Dosyayı Object Storage'a yükler
        /// </summary>
        /// <param name="bucketName">Bucket adı</param>
        /// <param name="objectName">StoredFileName (key)</param>
        /// <param name="data">Dosya byte verisi</param>
        /// <param name="contentType">Dosya tipi</param>
        /// <returns>StoredFileName (key)</returns>
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
    }
}