using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Activities;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
{
    public void Configure(EntityTypeBuilder<ActivityType> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ReferenceFormatRegex).HasMaxLength(200);
        builder.Property(a => a.ReferenceExample).HasMaxLength(50);
        builder.Property(a => a.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(a => a.Code).IsUnique();
    }
}
