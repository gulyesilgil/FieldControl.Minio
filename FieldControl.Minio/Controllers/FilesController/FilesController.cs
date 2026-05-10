using FieldControl.Minio.Services.StorageCleanup;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly StorageCleanupService _cleanupService;

        public FilesController(StorageCleanupService cleanupService)
        {
            _cleanupService = cleanupService;
        }

        //  DRY RUN
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
    }
}