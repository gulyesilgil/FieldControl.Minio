namespace FieldControl.Minio.DTOs.Inspection
{
    public class InspectionDto
    {
        public Guid Id { get; set; }

        public string ProductName { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string InspectorName { get; set; } = default!;

        public string Status { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}