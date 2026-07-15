using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Nom).IsRequired().HasMaxLength(200);
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(s => s.Code).IsUnique();

        builder.HasOne(s => s.Department)
            .WithMany(d => d.Services)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Responsable)
            .WithMany()
            .HasForeignKey(s => s.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
