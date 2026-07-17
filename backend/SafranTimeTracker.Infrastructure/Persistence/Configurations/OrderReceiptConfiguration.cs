using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OrderReceiptConfiguration : IEntityTypeConfiguration<OrderReceipt>
{
    public void Configure(EntityTypeBuilder<OrderReceipt> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReceivedAmount).HasPrecision(18, 2);
        builder.Property(r => r.ReceivedDays).HasPrecision(9, 2);
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.Comment).HasMaxLength(1000);
        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(r => r.OrderId);

        builder.HasOne(r => r.Order)
            .WithMany(o => o.Receipts)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
