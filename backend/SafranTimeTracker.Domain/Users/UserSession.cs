namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Session serveur de l'authentification simulée (Lot 13) : remplace le rejeu de l'en-tête
/// <c>X-Demo-User</c> sur chaque requête par un jeton de session opaque, révocable et expirable,
/// résolu par <c>IAuthenticationProvider</c>/<c>DemoAuthenticationProvider</c> — seule cette paire
/// connaît le mécanisme de démonstration, le reste de l'application ne dépend que d'<c>ICurrentUser</c>.
/// <see cref="IsPersistent"/> distingue dès ce lot une session navigateur (éphémère, expiration
/// glissante courte) d'une session persistante ("se souvenir de moi", expiration longue) — la
/// fonctionnalité de sélection utilisateur ("se souvenir de moi") n'est pas construite ce lot,
/// mais le modèle est prêt à la porter sans migration supplémentaire (voir CLAUDE.md §13).
/// </summary>
public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Session navigateur (false, cookie sans expiration explicite côté client, expiration
    /// serveur glissante courte) ou persistante ("se souvenir de moi", cookie à expiration longue
    /// explicite) — non exposée à l'utilisateur ce lot, toujours <c>false</c> à la création.</summary>
    public bool IsPersistent { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
