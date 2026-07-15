using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class HolidayCalendarConfiguration : IEntityTypeConfiguration<HolidayCalendar>
{
    public void Configure(EntityTypeBuilder<HolidayCalendar> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Libelle).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Pays).IsRequired().HasMaxLength(50);
        builder.Property(h => h.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(h => h.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(h => new { h.Pays, h.Date }).IsUnique();
    }
}
