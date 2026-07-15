using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class DashboardKpiConfiguration : IEntityTypeConfiguration<DashboardKpi>
{
    public void Configure(EntityTypeBuilder<DashboardKpi> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Code).IsRequired().HasMaxLength(30);
        builder.Property(k => k.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(k => k.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(k => k.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(k => k.Code).IsUnique();
    }
}
