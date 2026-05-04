using Microsoft.EntityFrameworkCore;
using System.Reflection;
using FieldControl.Minio.Entities;

namespace FieldControl.Minio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<InspectionFile> InspectionFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Daha önce yazdığın Configuration dosyalarını (sütun uzunlukları vb.) otomatik bağlar
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }
    }
}