using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Budgets;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Comment).HasMaxLength(1000);
        builder.Property(b => b.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(b => b.UpdatedBy).HasMaxLength(100);

        builder.Property(b => b.InitialAmount).HasPrecision(18, 2);
        builder.Property(b => b.AdjustedAmount).HasPrecision(18, 2);
        builder.Property(b => b.AlertThreshold).HasPrecision(5, 2);

        builder.HasOne(b => b.Project)
            .WithMany()
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Order)
            .WithMany()
            .HasForeignKey(b => b.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
