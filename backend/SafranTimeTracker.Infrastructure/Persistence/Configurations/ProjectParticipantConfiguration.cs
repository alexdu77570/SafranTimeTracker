using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectParticipantConfiguration : IEntityTypeConfiguration<ProjectParticipant>
{
    public void Configure(EntityTypeBuilder<ProjectParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CapacitePrevue).HasPrecision(9, 2);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.HasOne(p => p.Project)
            .WithMany(pr => pr.Participants)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Resource)
            .WithMany()
            .HasForeignKey(p => p.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.OperationalRole)
            .WithMany()
            .HasForeignKey(p => p.OperationalRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DefaultOrder)
            .WithMany()
            .HasForeignKey(p => p.DefaultOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.ProjectId, p.ResourceId });
    }
}
