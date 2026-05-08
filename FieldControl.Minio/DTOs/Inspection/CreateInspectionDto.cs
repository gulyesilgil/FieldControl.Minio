using System.ComponentModel.DataAnnotations;

namespace FieldControl.Minio.DTOs.Inspection
{
    public class CreateInspectionDto
    {
        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = default!;

        [Required] // 
        public string Description { get; set; } = default!;

        [Required]
        [MaxLength(150)]
        public string InspectorName { get; set; } = default!;

        [Required]
        public string Status { get; set; } = default!;
    }
}