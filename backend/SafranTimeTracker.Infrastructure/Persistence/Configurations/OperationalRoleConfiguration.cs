using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OperationalRoleConfiguration : IEntityTypeConfiguration<OperationalRole>
{
    public void Configure(EntityTypeBuilder<OperationalRole> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Libelle).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.Code).IsUnique();
    }
}
