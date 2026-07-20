using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Currencies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CodeIso).IsRequired().HasMaxLength(3);
        builder.Property(c => c.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Symbole).IsRequired().HasMaxLength(5);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(c => c.CodeIso).IsUnique();
    }
}
