using FieldControl.Minio.Entities;
using FieldControl.Minio.Services.InspectionAppService;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionsController : ControllerBase
    {
        private readonly InspectionAppService _service;

        public InspectionsController(InspectionAppService service)
        {
            _service = service;
        }

        // POST /api/inspections
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inspection inspection)
        {
            var result = await _service.CreateAsync(inspection);
            return Ok(result);
        }

        // GET /api/inspections
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET /api/inspections/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // PUT /api/inspections/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Inspection inspection)
        {
            var success = await _service.UpdateAsync(id, inspection);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/inspections/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}