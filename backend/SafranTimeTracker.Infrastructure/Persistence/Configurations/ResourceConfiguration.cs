using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nom).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Prenom).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Commentaire).HasMaxLength(1000);
        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);
        builder.Property(r => r.DailyCapacity).HasPrecision(5, 2);
        builder.Property(r => r.WeeklyCapacity).HasPrecision(5, 2);

        builder.HasOne(r => r.Department)
            .WithMany()
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Service)
            .WithMany()
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Team)
            .WithMany()
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResponsableHierarchique)
            .WithMany()
            .HasForeignKey(r => r.ResponsableHierarchiqueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Company)
            .WithMany()
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.DefaultOrder)
            .WithMany()
            .HasForeignKey(r => r.DefaultOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
