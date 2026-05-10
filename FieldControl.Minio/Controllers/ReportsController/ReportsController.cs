using FieldControl.Minio.Services.Report;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers.ReportsController
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _service;

        public ReportsController(ReportService service)
        {
            _service = service;
        }

        [HttpGet("inspection-excel")]
        public async Task<IActionResult> Export()
        {
            var file = await _service.ExportAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "report.xlsx"
            );
        }
    }
}