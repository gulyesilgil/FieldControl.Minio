using FieldControl.Minio.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldControl.Minio.Configurations
{
    public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
    {
        public void Configure(EntityTypeBuilder<Inspection> builder)
        {
            builder.ToTable("inspections");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.InspectorName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // İlişki tanımı
            builder.HasMany(x => x.InspectionFiles)
                .WithOne(x => x.Inspection)
                .HasForeignKey(x => x.InspectionId)
                .OnDelete(DeleteBehavior.Cascade); // Silme kuralını da ekledik
        }
    }
}
