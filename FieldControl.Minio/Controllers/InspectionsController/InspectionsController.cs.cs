using FieldControl.Minio.Data;
using FieldControl.Minio.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FieldControl.Minio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;

        public InspectionsController(AppDbContext context, IFileStorageService storageService)
        {
            _context = context;
            _storageService = storageService;

        }

        // POST /api/inspections
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inspection inspection)
        {
            inspection.Id = Guid.NewGuid();
            inspection.CreatedAt = DateTime.UtcNow;
            inspection.UpdatedAt = DateTime.UtcNow;

            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = inspection.Id }, inspection);
        }

        // GET /api/inspections
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inspections = await _context.Inspections
                .Include(i => i.InspectionFiles)
                .ToListAsync();

            return Ok(inspections);
        }

        // GET /api/inspections/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.InspectionFiles)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null)
                return NotFound();

            return Ok(inspection);
        }

        // PUT /api/inspections/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Inspection updated)
        {
            var inspection = await _context.Inspections.FindAsync(id);

            if (inspection == null)
                return NotFound();

            inspection.ProductName = updated.ProductName;
            inspection.Description = updated.Description;
            inspection.InspectorName = updated.InspectorName;
            inspection.Status = updated.Status;
            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/inspections/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var inspection = await _context.Inspections
                .Include(i => i.InspectionFiles)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inspection == null)
                return NotFound();

            foreach (var file in inspection.InspectionFiles)
            {
                await _storageService.DeleteFileAsync(
                    file.BucketName,
                    file.StoredFileName
                );
            }

            _context.InspectionFiles.RemoveRange(inspection.InspectionFiles);

            _context.Inspections.Remove(inspection);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}