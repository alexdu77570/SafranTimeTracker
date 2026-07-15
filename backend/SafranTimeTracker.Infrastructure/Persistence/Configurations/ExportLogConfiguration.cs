using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ExportLogConfiguration : IEntityTypeConfiguration<ExportLog>
{
    public void Configure(EntityTypeBuilder<ExportLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.GeneratedBy).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AppVersion).IsRequired().HasMaxLength(30);
        builder.Property(e => e.ReportType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FiltersJson).IsRequired();
        builder.HasIndex(e => e.GeneratedAt);
    }
}
