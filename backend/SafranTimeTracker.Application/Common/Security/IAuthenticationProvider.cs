namespace SafranTimeTracker.Application.Common.Security;

/// <summary>Identité résolue, sans les permissions (calculées séparément par
/// <see cref="PermissionResolutionService"/>, jamais dupliqué ici).</summary>
public sealed record ResolvedIdentity(Guid UserId, string Identifiant);

/// <summary>Résultat de la création d'une session (CLAUDE.md §17, Lot 13) : porte tout ce dont
/// l'appelant a besoin pour poser le cookie de session (expiration, caractère persistant).</summary>
public sealed record AuthenticationSession(Guid SessionId, Guid UserId, string Identifiant, DateTime ExpiresAt, bool IsPersistent);

/// <summary>
/// Cycle de vie de l'authentification, seule abstraction dont dépendent le middleware de résolution
/// d'identité et les contrôleurs d'authentification (CLAUDE.md §17, docs/ARCHITECTURE.md §4).
/// L'implémentation active (<c>DemoAuthenticationProvider</c>, Api, composition root) gère une
/// session simulée sans mot de passe ; elle est remplaçable par un futur provider LDAP/Active
/// Directory/OpenID Connect sans modifier un seul contrôleur, service applicatif ou règle
/// d'autorisation qui dépend de cette interface ou d'<see cref="ICurrentUser"/>.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>Établit une session pour l'identifiant donné (utilisateur actif requis). Retourne
    /// <c>null</c> si l'identifiant ne correspond à aucun utilisateur actif — jamais une exception,
    /// pour laisser le contrôleur choisir la réponse HTTP (401) sans fuiter d'information sur
    /// l'existence de l'identifiant.</summary>
    Task<AuthenticationSession?> CreateSessionAsync(string identifiant, bool isPersistent, CancellationToken cancellationToken = default);

    /// <summary>Révoque une session existante. Silencieux si elle n'existe pas ou est déjà révoquée
    /// (une déconnexion est toujours un succès du point de vue de l'appelant).</summary>
    Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>Résout une session par son jeton, à condition qu'elle soit active et non expirée ;
    /// prolonge son expiration glissante (LastActivityAt/ExpiresAt) si elle est valide. Retourne
    /// <c>null</c> sinon (jeton inconnu, révoqué, expiré, ou utilisateur devenu inactif).</summary>
    Task<ResolvedIdentity?> ResolveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>Résout directement un identifiant sans passer par une session — chemin de
    /// compatibilité réservé aux environnements Development/Test (<see cref="AuthenticationOptions.AllowDirectDemoHeader"/>),
    /// jamais emprunté en Qualification/Production.</summary>
    Task<ResolvedIdentity?> ResolveDirectIdentifierAsync(string identifiant, CancellationToken cancellationToken = default);
}
