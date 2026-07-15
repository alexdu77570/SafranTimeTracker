using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class CompanyTypeConfiguration : IEntityTypeConfiguration<CompanyType>
{
    public void Configure(EntityTypeBuilder<CompanyType> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Libelle).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
