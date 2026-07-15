using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nom).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Commentaire).HasMaxLength(1000);
        builder.Property(m => m.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(m => m.UpdatedBy).HasMaxLength(100);

        builder.HasOne(m => m.MilestoneType)
            .WithMany()
            .HasForeignKey(m => m.MilestoneTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Application)
            .WithMany()
            .HasForeignKey(m => m.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Responsable)
            .WithMany()
            .HasForeignKey(m => m.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DependsOnMilestone)
            .WithMany()
            .HasForeignKey(m => m.DependsOnMilestoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ProjectId, m.DatePrevue });
    }
}
