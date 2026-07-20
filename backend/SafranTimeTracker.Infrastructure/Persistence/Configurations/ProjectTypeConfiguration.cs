using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure(EntityTypeBuilder<ProjectType> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(t => t.Code).IsUnique();
    }
}
