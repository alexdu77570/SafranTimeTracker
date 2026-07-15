using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Applications;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ApplicationReferenceConfiguration : IEntityTypeConfiguration<ApplicationReference>
{
    public void Configure(EntityTypeBuilder<ApplicationReference> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nom).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Commentaire).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(a => a.Code).IsUnique();

        builder.HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Team)
            .WithMany()
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Responsable)
            .WithMany()
            .HasForeignKey(a => a.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
