using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ResourceCapacityPeriodConfiguration : IEntityTypeConfiguration<ResourceCapacityPeriod>
{
    public void Configure(EntityTypeBuilder<ResourceCapacityPeriod> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DailyCapacity).HasPrecision(5, 2);
        builder.Property(p => p.WeeklyCapacity).HasPrecision(5, 2);
        builder.Property(p => p.Reason).HasMaxLength(200);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.HasOne(p => p.Resource)
            .WithMany()
            .HasForeignKey(p => p.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.ResourceId, p.StartDate });
    }
}
