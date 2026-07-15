using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class CompanyContractHistoryConfiguration : IEntityTypeConfiguration<CompanyContractHistory>
{
    public void Configure(EntityTypeBuilder<CompanyContractHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.ContractNumber).HasMaxLength(50);
        builder.Property(h => h.ContractDailyRate).HasPrecision(18, 2);
        builder.Property(h => h.Currency).IsRequired().HasMaxLength(3);
        builder.Property(h => h.Comment).HasMaxLength(1000);
        builder.Property(h => h.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(h => h.UpdatedBy).HasMaxLength(100);

        // Jeton de concurrence optimiste géré applicativement (CLAUDE.md §11), portable sur les
        // 3 providers (docs/DATABASE.md §1).
        builder.Property(h => h.ConcurrencyStamp).IsConcurrencyToken();

        builder.HasOne(h => h.Company)
            .WithMany()
            .HasForeignKey(h => h.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => new { h.CompanyId, h.StartDate });
    }
}
