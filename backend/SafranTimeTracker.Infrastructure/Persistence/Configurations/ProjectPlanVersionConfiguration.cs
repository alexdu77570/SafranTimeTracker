using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectPlanVersionConfiguration : IEntityTypeConfiguration<ProjectPlanVersion>
{
    public void Configure(EntityTypeBuilder<ProjectPlanVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Motif).HasMaxLength(500);
        builder.Property(v => v.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(v => v.UpdatedBy).HasMaxLength(100);

        builder.HasOne(v => v.Project)
            .WithMany(p => p.PlanVersions)
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => new { v.ProjectId, v.Type, v.Statut });
    }
}
