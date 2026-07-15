using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Libelle).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.Code).IsUnique();
    }
}
