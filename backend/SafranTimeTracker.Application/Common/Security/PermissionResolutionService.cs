using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Common.Security;

/// <summary>
/// Calcul centralisé et déterministe des permissions effectives d'un utilisateur (modèle RBAC,
/// cahier des charges §6.1, Lot 13) — même principe que <c>FinancialCalculationService</c> ou
/// <c>ProjectPlanningCalculator</c> : une seule règle, jamais dupliquée. Formule :
/// <code>effectives = (permissions du rôle) ∪ (octrois individuels) − (retraits individuels)</code>
/// Les retraits individuels priment toujours sur le rôle. Réutilisé à la fois par la résolution
/// d'<see cref="ICurrentUser"/> (via le middleware d'identité) et par <c>UserService</c> (octroi/
/// retrait individuel, affichage de <c>UserDto.EffectivePermissionCodes</c>).
/// </summary>
public class PermissionResolutionService(
    IReadRepository<User> userRepository,
    IReadRepository<RolePermission> rolePermissionRepository,
    IReadRepository<UserPermission> userPermissionRepository,
    IReadRepository<Permission> permissionRepository)
{
    public async Task<IReadOnlyList<string>> GetEffectivePermissionCodesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleId = await userRepository.Query()
            .Where(u => u.Id == userId)
            .Select(u => (Guid?)u.RoleId)
            .FirstOrDefaultAsync(cancellationToken);
        if (roleId is null)
        {
            return [];
        }

        var rolePermissionCodes = await GetRolePermissionCodesAsync(roleId.Value, cancellationToken);

        var individual = await userPermissionRepository.Query()
            .Where(up => up.UserId == userId)
            .Join(permissionRepository.Query(), up => up.PermissionId, p => p.Id, (up, p) => new { p.Code, up.Effect })
            .ToListAsync(cancellationToken);

        var grants = individual.Where(x => x.Effect == UserPermissionEffect.Grant).Select(x => x.Code);
        var revokes = individual
            .Where(x => x.Effect == UserPermissionEffect.Revoke)
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return rolePermissionCodes
            .Concat(grants)
            .Where(code => !revokes.Contains(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Permissions accordées par le rôle seul, sans les exceptions individuelles — utilisé
    /// par <c>UserService</c> pour déterminer si un retrait individuel doit matérialiser une ligne
    /// de retrait explicite (le rôle accorde encore la permission) ou si une simple suppression de
    /// l'octroi individuel suffit (le rôle ne l'accorde pas).</summary>
    public async Task<IReadOnlyList<string>> GetRolePermissionCodesAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        await rolePermissionRepository.Query()
            .Where(rp => rp.RoleId == roleId)
            .Join(permissionRepository.Query(), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .ToListAsync(cancellationToken);
}
