using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ResourceCompanyAssignmentConfiguration : IEntityTypeConfiguration<ResourceCompanyAssignment>
{
    public void Configure(EntityTypeBuilder<ResourceCompanyAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssignmentType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Comment).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);

        builder.HasOne(a => a.Resource)
            .WithMany()
            .HasForeignKey(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Company)
            .WithMany()
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ResourceId, a.StartDate });
    }
}
