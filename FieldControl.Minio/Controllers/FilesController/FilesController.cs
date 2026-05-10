using FieldControl.Minio.Services.BucketService;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly BucketService _bucketService;

        public FilesController(BucketService bucketService)
        {
            _bucketService = bucketService;
        }

        [HttpGet("download-bucket")]
        public async Task<IActionResult> DownloadBucket()
        {
            var result = await _bucketService.DownloadEntireBucketAsync();

            if (result == null)
                return NotFound("Bucket is empty");

            return File(
                result.Value.ZipBytes,
                "application/zip",
                result.Value.FileName
            );
        }
    }
}