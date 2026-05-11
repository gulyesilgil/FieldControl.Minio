namespace FieldControl.Minio.DTOs.InspectionFile
{
    public class InspectionFileDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; } // 🔥 EKLENDİ
        public string FileName { get; set; } = default!;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}