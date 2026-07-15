using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Nom).IsRequired().HasMaxLength(200);
        builder.Property(d => d.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(d => d.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(d => d.Code).IsUnique();

        builder.HasOne(d => d.Responsable)
            .WithMany()
            .HasForeignKey(d => d.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
