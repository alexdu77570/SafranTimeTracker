using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nom).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Prenom).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Identifiant).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Telephone).HasMaxLength(30);
        builder.Property(u => u.Commentaire).HasMaxLength(1000);
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(u => u.UpdatedBy).HasMaxLength(100);
        builder.Property(u => u.SecurityLastModifiedBy).HasMaxLength(100);

        builder.HasIndex(u => u.Identifiant).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Resource)
            .WithMany()
            .HasForeignKey(u => u.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
