using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.User)
            .WithMany(u => u.UserSessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Résolution d'une session à chaque requête (CLAUDE.md §11 : index sur les clés de recherche
        // fréquentes) : UserId seul pour un futur "déconnecter partout", ExpiresAt pour le filtre de
        // validité systématiquement appliqué par DemoAuthenticationProvider.
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ExpiresAt);
    }
}
