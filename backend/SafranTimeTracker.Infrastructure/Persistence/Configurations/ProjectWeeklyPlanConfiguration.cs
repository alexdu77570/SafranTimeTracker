using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectWeeklyPlanConfiguration : IEntityTypeConfiguration<ProjectWeeklyPlan>
{
    public void Configure(EntityTypeBuilder<ProjectWeeklyPlan> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.ChargePlanifieeHeures).HasPrecision(6, 2);
        builder.Property(w => w.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(w => w.UpdatedBy).HasMaxLength(100);

        builder.HasOne(w => w.ProjectPlanVersion)
            .WithMany(v => v.WeeklyPlans)
            .HasForeignKey(w => w.ProjectPlanVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Resource)
            .WithMany()
            .HasForeignKey(w => w.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.ProjectPlanVersionId, w.ResourceId, w.WeekStartDate }).IsUnique();
    }
}
