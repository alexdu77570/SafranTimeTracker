using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Source).IsRequired().HasMaxLength(50);
        builder.Property(b => b.UserId).IsRequired().HasMaxLength(100);
        builder.Property(b => b.FileName).IsRequired().HasMaxLength(260);
        builder.Property(b => b.Checksum).IsRequired().HasMaxLength(64);

        builder.HasIndex(b => b.ImportDate);
        builder.HasIndex(b => new { b.Type, b.Source, b.Status });
    }
}

public class ImportDiffConfiguration : IEntityTypeConfiguration<ImportDiff>
{
    public void Configure(EntityTypeBuilder<ImportDiff> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FieldName).HasMaxLength(100);

        builder.HasIndex(d => d.ImportBatchId);

        builder.HasOne(d => d.ImportBatch)
            .WithMany(b => b.Diffs)
            .HasForeignKey(d => d.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
