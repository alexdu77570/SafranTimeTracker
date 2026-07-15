using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OrderAuthorizedResourceConfiguration : IEntityTypeConfiguration<OrderAuthorizedResource>
{
    public void Configure(EntityTypeBuilder<OrderAuthorizedResource> builder)
    {
        builder.HasKey(o => new { o.OrderId, o.ResourceId });

        builder.HasOne(o => o.Order)
            .WithMany(ord => ord.AuthorizedResources)
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Resource)
            .WithMany()
            .HasForeignKey(o => o.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
