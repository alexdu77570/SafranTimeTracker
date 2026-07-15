using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Budgets;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class BudgetVersionConfiguration : IEntityTypeConfiguration<BudgetVersion>
{
    public void Configure(EntityTypeBuilder<BudgetVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.OldValue).HasPrecision(18, 2);
        builder.Property(v => v.NewValue).HasPrecision(18, 2);
        builder.Property(v => v.Reason).IsRequired().HasMaxLength(500);
        builder.Property(v => v.ReferencePiece).HasMaxLength(200);
        builder.Property(v => v.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(v => v.BudgetId);

        builder.HasOne(v => v.Budget)
            .WithMany(b => b.Versions)
            .HasForeignKey(v => v.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
