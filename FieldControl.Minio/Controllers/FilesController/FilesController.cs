using FieldControl.Minio.Services.StorageCleanup;
using FieldControl.Minio.Services.BucketService;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly StorageCleanupService _cleanupService;
        private readonly BucketService _bucketService;

        public FilesController(
            StorageCleanupService cleanupService,
            BucketService bucketService)
        {
            _cleanupService = cleanupService;
            _bucketService = bucketService;
        }

        // CLEANUP 
        [HttpGet("cleanup")]
        public async Task<IActionResult> Cleanup([FromQuery] bool dryRun = true)
        {
            var result = await _cleanupService.CleanupAsync(dryRun);

            return Ok(new
            {
                dryRun,
                orphanCount = result.Count,
                orphanFiles = result
            });
        }

        //TÜM BUCKET DOWNLOAD (STREAM)
        [HttpGet("download-bucket")]
        public async Task DownloadBucket()
        {
            Response.ContentType = "application/zip";
            Response.Headers.Add("Content-Disposition", "attachment; filename=bucket.zip");

            await _bucketService.StreamBucketAsZipAsync(Response.Body);

            await Response.Body.FlushAsync(); // 🔥 EKLE
        }
    }
}