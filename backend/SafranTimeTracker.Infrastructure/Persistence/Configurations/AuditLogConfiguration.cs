using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Auditing;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Author).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Reason).HasMaxLength(1000);
        builder.Property(a => a.TechnicalContext).HasMaxLength(100);

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.Author);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
