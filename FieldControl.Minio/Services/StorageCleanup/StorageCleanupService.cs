using FieldControl.Minio.Data;
using FieldControl.Minio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FieldControl.Minio.Services.StorageCleanup
{
    public class StorageCleanupService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly string _bucketName;

        public StorageCleanupService(
            AppDbContext context,
            IFileStorageService storageService,
            IConfiguration config)
        {
            _context = context;
            _storageService = storageService;
            _bucketName = config["MinioSettings:BucketName"]!;
        }

        public async Task<List<string>> CleanupAsync(bool dryRun)
        {
            //  1. DB'deki tüm StoredFileName'ler
            var dbKeys = await _context.InspectionFiles
                .Select(f => f.StoredFileName)
                .ToListAsync();

            var dbSet = new HashSet<string>(dbKeys);

            //  2. MinIO’daki tüm object key’ler
            var bucketKeys = await _storageService.ListFilesAsync(_bucketName);

            var orphanList = new List<string>();

            //  3. karşılaştır
            foreach (var key in bucketKeys)
            {
                if (!dbSet.Contains(key))
                {
                    orphanList.Add(key);

                    //  4. sil (dryRun değilse)
                    if (!dryRun)
                    {
                        try
                        {
                            await _storageService.DeleteFileAsync(_bucketName, key);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Delete failed: {key} → {ex.Message}");
                        }
                    }
                }
            }

            return orphanList;
        }
    }
}