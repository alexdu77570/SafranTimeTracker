using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Reference).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Libelle).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Commentaire).HasMaxLength(1000);
        builder.Property(o => o.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(o => o.UpdatedBy).HasMaxLength(100);

        builder.Property(o => o.BudgetFinancierInitial).HasPrecision(18, 2);
        builder.Property(o => o.BudgetJoursInitial).HasPrecision(9, 2);
        builder.Property(o => o.SeuilAlerte).HasPrecision(5, 2);

        builder.HasIndex(o => o.Reference).IsUnique();

        builder.HasOne(o => o.Company)
            .WithMany()
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Status)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
