namespace SafranTimeTracker.Application.Common.Security;

/// <summary>
/// Identité de l'appelant courant, seule abstraction dont dépendent les contrôleurs et services
/// applicatifs pour vérifier une permission ou tracer un auteur (CLAUDE.md §17,
/// docs/ARCHITECTURE.md §4). L'implémentation active est un détail d'assemblage (Api,
/// composition root) : elle peut être remplacée par un provider LDAP/OIDC sans modifier un seul
/// contrôleur, service métier ou règle de sécurité qui dépend de cette interface.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }

    /// <summary>Identifiant à utiliser pour CreatedBy/UpdatedBy. Retombe sur
    /// <see cref="CurrentActor.PlaceholderIdentifier"/> quand <see cref="IsAuthenticated"/> est faux.</summary>
    string Identifier { get; }

    IReadOnlyCollection<string> Permissions { get; }
    bool HasPermission(string permissionCode);
}
