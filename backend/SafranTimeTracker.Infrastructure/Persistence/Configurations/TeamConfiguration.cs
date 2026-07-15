using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Nom).IsRequired().HasMaxLength(200);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasOne(t => t.Service)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ResponsableFonctionnel)
            .WithMany()
            .HasForeignKey(t => t.ResponsableFonctionnelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
