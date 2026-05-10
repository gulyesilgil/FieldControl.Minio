using ClosedXML.Excel;
using FieldControl.Minio.Data;
using FieldControl.Minio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FieldControl.Minio.Services.Report
{
    public class ReportService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName;

        public ReportService(
            AppDbContext context,
            IFileStorageService storageService,
            IConfiguration config)
        {
            _context = context;
            _storageService = storageService;
            _bucketName = config["MinioSettings:BucketName"]!;
        }

        public async Task<byte[]> ExportAsync()
        {
            var inspections = await _context.Inspections
                .Include(i => i.InspectionFiles)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Report");

            // HEADER
            ws.Cell(1, 1).Value = "InspectionId";
            ws.Cell(1, 2).Value = "ProductName";
            ws.Cell(1, 3).Value = "Description";
            ws.Cell(1, 4).Value = "InspectorName";
            ws.Cell(1, 5).Value = "Status";
            ws.Cell(1, 6).Value = "FileId";
            ws.Cell(1, 7).Value = "FileName";
            ws.Cell(1, 8).Value = "Image";

            int row = 2;

            foreach (var inspection in inspections)
            {
                foreach (var file in inspection.InspectionFiles)
                {
                    ws.Cell(row, 1).Value = inspection.Id.ToString();
                    ws.Cell(row, 2).Value = inspection.ProductName;
                    ws.Cell(row, 3).Value = inspection.Description;
                    ws.Cell(row, 4).Value = inspection.InspectorName;
                    ws.Cell(row, 5).Value = inspection.Status.ToString();
                    ws.Cell(row, 6).Value = file.Id.ToString();
                    ws.Cell(row, 7).Value = file.FileName;

                    // IMAGE EKLE (SAFE)
                    try
                    {
                        if (!string.IsNullOrEmpty(file.StoredFileName) &&
                            file.ContentType.StartsWith("image"))
                        {
                            var bytes = await _storageService.DownloadFileAsync(
                                _bucketName,
                                file.StoredFileName
                            );

                            using var stream = new MemoryStream(bytes);

                            ws.AddPicture(stream)
                              .MoveTo(ws.Cell(row, 8))
                              .Scale(0.4);

                            ws.Row(row).Height = 90;
                        }
                    }
                    catch
                    {
                        // önemli: hata olursa geç
                    }

                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}