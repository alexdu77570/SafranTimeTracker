using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Api.Security;

/// <summary>
/// Implémentation de démonstration de <see cref="ICurrentUser"/> (MVP, CLAUDE.md §17) : résout
/// l'utilisateur courant à partir de l'en-tête <see cref="DemoUserHeaderName"/>, vérifié en base
/// contre les entités User/UserPermission/Permission réelles — ce n'est ni un login, ni un JWT,
/// ni une session, ni un cookie, ni ASP.NET Identity. Seul ce fichier (et son enregistrement DI
/// dans Program.cs) connaît le mécanisme de démonstration ; tout le reste de l'application ne
/// dépend que de ICurrentUser. Remplacer ce provider par un provider LDAP/OIDC futur ne touche ni
/// les contrôleurs, ni les services applicatifs, ni les règles d'autorisation.
/// </summary>
public sealed class DemoCurrentUserProvider : ICurrentUser
{
    public const string DemoUserHeaderName = "X-Demo-User";

    private readonly Lazy<ResolvedUser?> _resolved;

    public DemoCurrentUserProvider(
        IHttpContextAccessor httpContextAccessor,
        IReadRepository<User> userRepository,
        IReadRepository<UserPermission> userPermissionRepository,
        IReadRepository<Permission> permissionRepository)
    {
        _resolved = new Lazy<ResolvedUser?>(() => Resolve(httpContextAccessor, userRepository, userPermissionRepository, permissionRepository));
    }

    public bool IsAuthenticated => _resolved.Value is not null;
    public Guid? UserId => _resolved.Value?.UserId;
    public string Identifier => _resolved.Value?.Identifiant ?? CurrentActor.PlaceholderIdentifier;
    public IReadOnlyCollection<string> Permissions => _resolved.Value?.PermissionCodes ?? [];

    public bool HasPermission(string permissionCode) =>
        Permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);

    private static ResolvedUser? Resolve(
        IHttpContextAccessor httpContextAccessor,
        IReadRepository<User> userRepository,
        IReadRepository<UserPermission> userPermissionRepository,
        IReadRepository<Permission> permissionRepository)
    {
        var headerValue = httpContextAccessor.HttpContext?.Request.Headers[DemoUserHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return null;
        }

        var user = userRepository.Query()
            .Where(u => u.Identifiant == headerValue && u.Statut == ReferentialStatus.Actif)
            .Select(u => new { u.Id, u.Identifiant })
            .FirstOrDefault();
        if (user is null)
        {
            return null;
        }

        var permissionIds = userPermissionRepository.Query()
            .Where(up => up.UserId == user.Id)
            .Select(up => up.PermissionId)
            .ToList();

        var permissionCodes = permissionRepository.Query()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .ToList();

        return new ResolvedUser(user.Id, user.Identifiant, permissionCodes);
    }

    private sealed record ResolvedUser(Guid UserId, string Identifiant, IReadOnlyCollection<string> PermissionCodes);
}
