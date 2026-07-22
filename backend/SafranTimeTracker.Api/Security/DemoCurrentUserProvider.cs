using SafranTimeTracker.Api.Middleware;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Security;

/// <summary>
/// Implémentation de démonstration de <see cref="ICurrentUser"/> (MVP, CLAUDE.md §17, Lot 13) :
/// lit l'identité déjà résolue par <see cref="IdentityResolutionMiddleware"/> (session cookie, avec
/// repli Development/Test sur l'en-tête <see cref="DemoUserHeaderName"/>) dans
/// <see cref="HttpContext.Items"/> — aucun accès base de données ici, tout le travail asynchrone a
/// déjà eu lieu une fois par requête dans le middleware. Seuls ce fichier, le middleware et
/// <c>DemoAuthenticationProvider</c> connaissent le mécanisme de démonstration ; le reste de
/// l'application ne dépend que d'<see cref="ICurrentUser"/>. Remplacer ce provider par un provider
/// LDAP/OIDC futur ne touche ni les contrôleurs, ni les services applicatifs, ni les règles
/// d'autorisation.
/// </summary>
public sealed class DemoCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public const string DemoUserHeaderName = "X-Demo-User";

    private ResolvedCurrentIdentity? Resolved =>
        httpContextAccessor.HttpContext?.Items[IdentityResolutionMiddleware.ItemsKey] as ResolvedCurrentIdentity;

    public bool IsAuthenticated => Resolved is not null;
    public Guid? UserId => Resolved?.UserId;
    public string Identifier => Resolved?.Identifiant ?? CurrentActor.PlaceholderIdentifier;
    public IReadOnlyCollection<string> Permissions => Resolved?.PermissionCodes ?? [];

    public bool HasPermission(string permissionCode) =>
        Permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
}
