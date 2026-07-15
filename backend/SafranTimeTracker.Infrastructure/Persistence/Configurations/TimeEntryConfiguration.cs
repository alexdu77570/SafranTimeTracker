using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.DureeHeures).HasPrecision(5, 2);
        builder.Property(t => t.Reference).HasMaxLength(50);
        builder.Property(t => t.Commentaire).HasMaxLength(1000);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);

        builder.HasOne(t => t.Resource)
            .WithMany()
            .HasForeignKey(t => t.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ActivityType)
            .WithMany()
            .HasForeignKey(t => t.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Order)
            .WithMany()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FinancialSnapshot)
            .WithOne(s => s.TimeEntry)
            .HasForeignKey<TimeEntryFinancialSnapshot>(s => s.TimeEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.ResourceId, t.Date });
    }
}
