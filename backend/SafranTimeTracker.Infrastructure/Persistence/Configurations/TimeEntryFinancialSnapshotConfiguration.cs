using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class TimeEntryFinancialSnapshotConfiguration : IEntityTypeConfiguration<TimeEntryFinancialSnapshot>
{
    public void Configure(EntityTypeBuilder<TimeEntryFinancialSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TjmPersonneSnapshot).HasPrecision(18, 2);
        builder.Property(s => s.TjmContratSnapshot).HasPrecision(18, 2);
        builder.Property(s => s.CoutReelCalcule).HasPrecision(18, 2);
        builder.Property(s => s.CoutContratCalcule).HasPrecision(18, 2);
        builder.Property(s => s.DifferentielCalcule).HasPrecision(18, 2);
        builder.Property(s => s.SourceTjmPersonne).HasMaxLength(50);
        builder.Property(s => s.SourceContrat).HasMaxLength(50);
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(s => s.TimeEntryId).IsUnique();
    }
}
