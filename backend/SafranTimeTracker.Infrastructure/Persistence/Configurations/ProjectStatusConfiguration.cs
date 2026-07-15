using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectStatusConfiguration : IEntityTypeConfiguration<ProjectStatus>
{
    public void Configure(EntityTypeBuilder<ProjectStatus> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Libelle).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Code).IsUnique();
    }
}
