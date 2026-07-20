using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Service)
            .WithMany()
            .HasForeignKey(c => c.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
