using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OrderExtensionConfiguration : IEntityTypeConfiguration<OrderExtension>
{
    public void Configure(EntityTypeBuilder<OrderExtension> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AmountAdded).HasPrecision(18, 2);
        builder.Property(e => e.DaysAdded).HasPrecision(9, 2);
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Comment).HasMaxLength(1000);
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(e => e.OrderId);

        builder.HasOne(e => e.Order)
            .WithMany(o => o.Extensions)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
