using FieldControl.Minio.DTOs.Inspection;
using FieldControl.Minio.Services.Inspection;
using Microsoft.AspNetCore.Mvc;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/inspections")]
    public class InspectionsController : ControllerBase
    {
        private readonly InspectionService _service;

        public InspectionsController(InspectionService service)
        {
            _service = service;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateInspectionDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET BY ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // UPDATE (FULL)
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, CreateInspectionDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // PATCH STATUS
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            UpdateInspectionStatusDto dto)
        {
            var success = await _service.UpdateStatusAsync(id, dto.Status);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // GET ALL STATUSES
        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            var result = _service.GetAllStatuses();
            return Ok(result);
        }

        // DELETE
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}