using FieldControl.Minio.Enums;

namespace FieldControl.Minio.Entities
{
    public class Inspection
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string InspectorName { get; set; }
        public InspectionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<InspectionFile> InspectionFiles { get; set; } = new();
        //inspection, inspection_files'dan id foriegn key'i alir
        //bu bir collection navigationdir
    }
}
