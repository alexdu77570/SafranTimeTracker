using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ResourceOperationalRoleConfiguration : IEntityTypeConfiguration<ResourceOperationalRole>
{
    public void Configure(EntityTypeBuilder<ResourceOperationalRole> builder)
    {
        builder.HasKey(r => new { r.ResourceId, r.OperationalRoleId });

        builder.HasOne(r => r.Resource)
            .WithMany(res => res.OperationalRoles)
            .HasForeignKey(r => r.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.OperationalRole)
            .WithMany(o => o.ResourceOperationalRoles)
            .HasForeignKey(r => r.OperationalRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
