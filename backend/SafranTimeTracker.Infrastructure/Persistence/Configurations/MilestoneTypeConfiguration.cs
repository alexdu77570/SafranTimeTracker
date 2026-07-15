using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class MilestoneTypeConfiguration : IEntityTypeConfiguration<MilestoneType>
{
    public void Configure(EntityTypeBuilder<MilestoneType> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(t => t.Code).IsUnique();
    }
}
