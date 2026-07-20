using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Technologies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(t => t.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(t => t.Code).IsUnique();
    }
}

public class ApplicationTechnologyConfiguration : IEntityTypeConfiguration<ApplicationTechnology>
{
    public void Configure(EntityTypeBuilder<ApplicationTechnology> builder)
    {
        builder.HasKey(at => new { at.ApplicationId, at.TechnologyId });

        builder.HasOne(at => at.Application)
            .WithMany()
            .HasForeignKey(at => at.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(at => at.Technology)
            .WithMany(t => t.Applications)
            .HasForeignKey(at => at.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResourceTechnologyConfiguration : IEntityTypeConfiguration<ResourceTechnology>
{
    public void Configure(EntityTypeBuilder<ResourceTechnology> builder)
    {
        builder.HasKey(rt => new { rt.ResourceId, rt.TechnologyId });

        builder.HasOne(rt => rt.Resource)
            .WithMany()
            .HasForeignKey(rt => rt.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.Technology)
            .WithMany(t => t.Resources)
            .HasForeignKey(rt => rt.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
