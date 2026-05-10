using Microsoft.AspNetCore.Mvc;
using FieldControl.Minio.Services.InspectionFileService;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/inspections/{inspectionId:guid}/files")]
    public class InspectionFilesController : ControllerBase
    {
        private readonly InspectionFileService _service;

        public InspectionFilesController(InspectionFileService service)
        {
            _service = service;
        }

        // UPLOAD
        [HttpPost]
        public async Task<IActionResult> Upload(
            Guid inspectionId,
            [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var result = await _service.UploadFilesAsync(inspectionId, files);

            return Ok(result);
        }

        // GET FILES
        [HttpGet]
        public async Task<IActionResult> GetFiles(Guid inspectionId)
        {
            var result = await _service.GetFilesAsync(inspectionId);
            return Ok(result);
        }

        // DOWNLOAD
        [HttpGet("{fileId:guid}/download")]
        public async Task<IActionResult> Download(Guid inspectionId, Guid fileId)
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

        // DELETE FILE
        [HttpDelete("{fileId:guid}")]
        public async Task<IActionResult> Delete(Guid inspectionId, Guid fileId)
        {
            var success = await _service.DeleteFileAsync(inspectionId, fileId);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // DOWNLOAD ALL (ZIP)
        [HttpGet("download-all")]
        public async Task<IActionResult> DownloadAll(Guid inspectionId)
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