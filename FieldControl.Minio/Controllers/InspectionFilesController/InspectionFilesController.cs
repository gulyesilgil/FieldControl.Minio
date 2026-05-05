using FieldControl.Minio.Services.InspectionAppService;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/inspections/{inspectionId:guid}/files")]
    public class InspectionFilesController : ControllerBase
    {
        private readonly InspectionAppService _service;

        public InspectionFilesController(InspectionAppService service)
        {
            _service = service;
        }

        // POST /api/inspections/{inspectionId}/files
        [HttpPost]
        public async Task<IActionResult> Upload(
            [FromRoute] Guid inspectionId,
            [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var result = await _service.UploadFilesAsync(inspectionId, files);

            if (!result)
                return NotFound("Inspection not found");

            return Ok();
        }

        // GET /api/inspections/{inspectionId}/files
        [HttpGet]
        public async Task<IActionResult> GetFiles([FromRoute] Guid inspectionId)
        {
            var result = await _service.GetFilesAsync(inspectionId);

            if (result == null || result.Count == 0)
                return NotFound("No files found");

            return Ok(result);
        }

        // GET /api/inspections/{inspectionId}/files/{fileId}/download
        [HttpGet("{fileId:guid}/download")]
        public async Task<IActionResult> Download(
            [FromRoute] Guid inspectionId,
            [FromRoute] Guid fileId)
        {
            var result = await _service.DownloadFileAsync(inspectionId, fileId);

            if (result == null)
                return NotFound();

            return File(
                result.Value.FileBytes,
                result.Value.ContentType,
                result.Value.FileName,
                enableRangeProcessing: true
            );
        }

        // DELETE /api/inspections/{inspectionId}/files/{fileId}
        [HttpDelete("{fileId:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid inspectionId,
            [FromRoute] Guid fileId)
        {
            var result = await _service.DeleteFileAsync(inspectionId, fileId);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // GET /api/inspections/{inspectionId}/files/download-all
        [HttpGet("download-all")]
        public async Task<IActionResult> DownloadAll([FromRoute] Guid inspectionId)
        {
            var result = await _service.ExportFilesAsZipAsync(inspectionId);

            if (result == null)
                return NotFound("No files found");

            return File(
                result.Value.ZipBytes,
                "application/zip",
                result.Value.FileName,
                enableRangeProcessing: true
            );
        }
    }
}