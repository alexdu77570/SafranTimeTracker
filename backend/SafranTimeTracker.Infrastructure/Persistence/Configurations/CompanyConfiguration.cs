using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nom).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ContactPrincipal).IsRequired().HasMaxLength(200);
        builder.Property(c => c.EmailContact).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Telephone).HasMaxLength(30);
        builder.Property(c => c.Adresse).HasMaxLength(500);
        builder.Property(c => c.Commentaire).HasMaxLength(1000);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasOne(c => c.CompanyType)
            .WithMany(t => t.Companies)
            .HasForeignKey(c => c.CompanyTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
