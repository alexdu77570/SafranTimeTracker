using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Clients;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Nom).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Commentaire).HasMaxLength(1000);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
