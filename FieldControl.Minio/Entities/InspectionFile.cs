namespace FieldControl.Minio.Entities
{
        public class InspectionFile
        {
            public Guid Id { get; set; }
            public Guid InspectionId { get; set; }
            public string FileName { get; set; }
            public string StoredFileName { get; set; }
            public string ContentType { get; set; }
            public long FileSize { get; set; }
            public string BucketName { get; set; } = default!;
            public DateTime CreatedAt { get; set; }

            public Inspection Inspection { get; set; }
            //inspection_files, inspection'dan id foriegn key'i alir
            //bu reference navigationdir
        }
    }

