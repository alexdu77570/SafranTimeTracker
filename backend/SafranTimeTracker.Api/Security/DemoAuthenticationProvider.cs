using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Api.Security;

/// <summary>
/// Implémentation de démonstration d'<see cref="IAuthenticationProvider"/> (MVP, CLAUDE.md §17,
/// Lot 13) : gère des sessions <see cref="UserSession"/> persistées en base, sans mot de passe ni
/// JWT — ce n'est ni un login réel, ni ASP.NET Identity. Seul ce fichier (et son enregistrement DI
/// dans Program.cs) connaît le mécanisme de démonstration ; tout le reste de l'application ne
/// dépend que d'<see cref="IAuthenticationProvider"/> et d'<see cref="ICurrentUser"/>. Remplacer ce
/// provider par un provider LDAP/OIDC futur ne touche ni les contrôleurs, ni les services
/// applicatifs, ni les règles d'autorisation.
/// </summary>
public sealed class DemoAuthenticationProvider(
    IRepository<UserSession> sessionRepository,
    IReadRepository<User> userRepository,
    IOptions<AuthenticationOptions> options) : IAuthenticationProvider
{
    public async Task<AuthenticationSession?> CreateSessionAsync(string identifiant, bool isPersistent, CancellationToken cancellationToken = default)
    {
        var user = await FindActiveUserAsync(identifiant, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsPersistent = isPersistent,
            CreatedAt = now,
            LastActivityAt = now,
            ExpiresAt = now.Add(LifetimeFor(isPersistent)),
        };

        await sessionRepository.AddAsync(session, cancellationToken);
        await sessionRepository.SaveChangesAsync(cancellationToken);

        return new AuthenticationSession(session.Id, user.Id, user.Identifiant, session.ExpiresAt, isPersistent);
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return;
        }

        session.RevokedAt = DateTime.UtcNow;
        await sessionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ResolvedIdentity?> ResolveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // Entité suivie (GetByIdAsync, CLAUDE.md §11) : l'expiration glissante ci-dessous mute une
        // entité déjà persistée, contrairement à Query() (AsNoTracking).
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        var now = DateTime.UtcNow;
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            return null;
        }

        var user = await userRepository.Query()
            .Where(u => u.Id == session.UserId && u.Statut == ReferentialStatus.Actif)
            .Select(u => new { u.Id, u.Identifiant })
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        // Expiration glissante : la session reste valide tant qu'elle est active.
        session.LastActivityAt = now;
        session.ExpiresAt = now.Add(LifetimeFor(session.IsPersistent));
        await sessionRepository.SaveChangesAsync(cancellationToken);

        return new ResolvedIdentity(user.Id, user.Identifiant);
    }

    public async Task<ResolvedIdentity?> ResolveDirectIdentifierAsync(string identifiant, CancellationToken cancellationToken = default)
    {
        var user = await FindActiveUserAsync(identifiant, cancellationToken);
        return user is null ? null : new ResolvedIdentity(user.Id, user.Identifiant);
    }

    private Task<UserProjection?> FindActiveUserAsync(string identifiant, CancellationToken cancellationToken) =>
        userRepository.Query()
            .Where(u => u.Identifiant == identifiant && u.Statut == ReferentialStatus.Actif)
            .Select(u => new UserProjection(u.Id, u.Identifiant))
            .FirstOrDefaultAsync(cancellationToken);

    private TimeSpan LifetimeFor(bool isPersistent) => isPersistent
        ? TimeSpan.FromDays(options.Value.PersistentSessionLifetimeDays)
        : TimeSpan.FromMinutes(options.Value.SessionLifetimeMinutes);

    private sealed record UserProjection(Guid Id, string Identifiant);
}
