using FieldControl.Minio.Data;
using FieldControl.Minio.DTOs.Inspection;
using FieldControl.Minio.Enums;
using Microsoft.EntityFrameworkCore;

// alias 
using InspectionEntity = FieldControl.Minio.Entities.Inspection;

namespace FieldControl.Minio.Services.Inspection
{
    public class InspectionService
    {
        private readonly AppDbContext _context;

        public InspectionService(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        public async Task<InspectionDto> CreateAsync(CreateInspectionDto dto)
        {
            var inspection = new InspectionEntity
            {
                Id = Guid.NewGuid(),
                ProductName = dto.ProductName,
                Description = dto.Description,
                InspectorName = dto.InspectorName,
                Status = ParseStatus(dto.Status),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();

            return MapToDto(inspection);
        }

        // GET ALL
        public async Task<List<InspectionDto>> GetAllAsync()
        {
            var inspections = await _context.Inspections.ToListAsync();
            return inspections.Select(MapToDto).ToList();
        }

        // GET BY ID
        public async Task<InspectionDto?> GetByIdAsync(Guid id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            return inspection == null ? null : MapToDto(inspection);
        }

        // UPDATE (FULL)
        public async Task<bool> UpdateAsync(Guid id, CreateInspectionDto dto)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return false;

            inspection.ProductName = dto.ProductName;
            inspection.Description = dto.Description;
            inspection.InspectorName = dto.InspectorName;
            inspection.Status = ParseStatus(dto.Status);
            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // PATCH STATUS
        public async Task<bool> UpdateStatusAsync(Guid id, string status)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return false;

            inspection.Status = ParseStatus(status);
            inspection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // GET ALL STATUSES
        public List<string> GetAllStatuses()
        {
            return Enum.GetNames(typeof(InspectionStatus)).ToList();
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null) return false;

            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();
            return true;
        }

        //  SAFE ENUM PARSE
        private InspectionStatus ParseStatus(string status)
        {
            if (!Enum.TryParse<InspectionStatus>(status, true, out var parsed))
                throw new ArgumentException($"Invalid status: {status}");

            return parsed;
        }

        //  MAPPING
        private InspectionDto MapToDto(InspectionEntity inspection)
        {
            return new InspectionDto
            {
                Id = inspection.Id,
                ProductName = inspection.ProductName,
                Description = inspection.Description,
                InspectorName = inspection.InspectorName,
                Status = inspection.Status.ToString(),
                CreatedAt = inspection.CreatedAt,
                UpdatedAt = inspection.UpdatedAt
            };
        }
    }
}