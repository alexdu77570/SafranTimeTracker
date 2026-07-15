using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Libelle).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Code).IsUnique();
    }
}
