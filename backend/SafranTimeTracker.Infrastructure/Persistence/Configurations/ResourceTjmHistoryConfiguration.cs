using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ResourceTjmHistoryConfiguration : IEntityTypeConfiguration<ResourceTjmHistory>
{
    public void Configure(EntityTypeBuilder<ResourceTjmHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.DailyRate).HasPrecision(18, 2);
        builder.Property(h => h.Reason).HasMaxLength(200);
        builder.Property(h => h.Comment).HasMaxLength(1000);
        builder.Property(h => h.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(h => h.UpdatedBy).HasMaxLength(100);

        // Jeton de concurrence optimiste géré applicativement (CLAUDE.md §11), pas un rowversion
        // natif : reste portable sur les 3 providers (docs/DATABASE.md §1).
        builder.Property(h => h.ConcurrencyStamp).IsConcurrencyToken();

        builder.HasOne(h => h.Resource)
            .WithMany()
            .HasForeignKey(h => h.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.ResourceId, h.StartDate });
    }
}
