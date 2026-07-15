using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Absences;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Commentaire).HasMaxLength(1000);
        builder.Property(a => a.ValideParIdentifiant).HasMaxLength(100);
        builder.Property(a => a.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);

        builder.HasOne(a => a.Resource)
            .WithMany()
            .HasForeignKey(a => a.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ResourceId, a.DateDebut });
    }
}
