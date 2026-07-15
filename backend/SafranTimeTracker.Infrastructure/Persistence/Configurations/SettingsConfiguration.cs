using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class SettingsConfiguration : IEntityTypeConfiguration<SettingsEntity>
{
    public void Configure(EntityTypeBuilder<SettingsEntity> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.PaysParDefaut).IsRequired().HasMaxLength(50);
        builder.Property(s => s.DeviseParDefaut).IsRequired().HasMaxLength(3);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);

        builder.Property(s => s.HeuresParJour).HasPrecision(4, 2);
        builder.Property(s => s.SeuilSurcharge).HasPrecision(5, 2);
        builder.Property(s => s.SeuilSousCharge).HasPrecision(5, 2);
        builder.Property(s => s.SeuilAlerteBudget).HasPrecision(5, 2);
        builder.Property(s => s.SeuilAlerteCommande).HasPrecision(5, 2);
    }
}
