using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nom).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.DescriptionCourte).HasMaxLength(500);
        builder.Property(p => p.Commentaire).HasMaxLength(1000);
        builder.Property(p => p.BudgetInitial).HasPrecision(18, 2);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(p => p.Code).IsUnique();

        builder.HasOne(p => p.Application)
            .WithMany()
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Pilote)
            .WithMany()
            .HasForeignKey(p => p.PiloteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Service)
            .WithMany()
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Team)
            .WithMany()
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Status)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProjectType)
            .WithMany()
            .HasForeignKey(p => p.ProjectTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
