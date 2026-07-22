using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users.Services;

/// <summary>
/// Cahier des charges §10, §28.3. Les opérations de sécurité (changement de rôle, octroi/retrait
/// de permission, désactivation) appliquent le garde-fou CLAUDE.md §17 : impossible de retirer le
/// dernier accès Administrateur actif de l'application. Toutes sont auditées via
/// <see cref="AuditService"/> — <see cref="CreateAsync"/> (Lot 1) reste inchangé : il précède
/// l'introduction d'<see cref="ICurrentUser"/> (Lot 2) et d'AuditLog (Lot 6), voir
/// docs/IMPLEMENTATION_STATUS.md.
/// </summary>
public class UserService(
    IRepository<User> repository,
    IReadRepository<Role> roleRepository,
    IReadRepository<Permission> permissionRepository,
    IRepository<UserPermission> userPermissionRepository,
    PermissionResolutionService permissionResolutionService,
    AuditService auditService,
    ICurrentUser currentUser)
{
    private const string AdministrateurRoleCode = "ADMINISTRATEUR";

    public async Task<PagedResult<UserDto>> GetListAsync(
        PaginationQuery pagination, Guid? roleId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (roleId is not null)
        {
            query = query.Where(u => u.RoleId == roleId);
        }
        if (statut is not null)
        {
            query = query.Where(u => u.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.Nom).ThenBy(u => u.Prenom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<UserDto>()
            .ToListAsync(cancellationToken);

        // Calcul RBAC par utilisateur (13 utilisateurs de démonstration à ce jour) : même principe
        // d'acceptabilité N+1 que la capacité par ressource du Lot 9, non anticipé au-delà.
        foreach (var item in items)
        {
            item.EffectivePermissionCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(item.Id, cancellationToken);
        }

        return new PagedResult<UserDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await repository.Query().Where(u => u.Id == id).ProjectToType<UserDto>().FirstOrDefaultAsync(cancellationToken);
        if (dto is not null)
        {
            dto.EffectivePermissionCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(dto.Id, cancellationToken);
        }
        return dto;
    }

    public async Task<UserDto> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entity = request.Adapt<User>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = now;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;
        entity.SecurityLastModifiedAt = now;
        entity.SecurityLastModifiedBy = CurrentActor.PlaceholderIdentifier;
        entity.UserPermissions = request.PermissionIds
            .Select(permissionId => new UserPermission
            {
                UserId = entity.Id,
                PermissionId = permissionId,
                GrantedAt = now,
                GrantedBy = CurrentActor.PlaceholderIdentifier
            })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    /// <summary>§28.3 "modification d'un utilisateur". Identifiant/rôle/permissions exclus, voir
    /// <see cref="UserUpdateRequest"/>.</summary>
    public async Task<UserDto?> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<UserDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(AuditActions.Update, nameof(User), id, oldValue, entity.Adapt<UserDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    /// <summary>§28.3 "désactivation d'un utilisateur". Statut plutôt que suppression physique
    /// (CLAUDE.md §7). Bloqué si l'utilisateur est le dernier Administrateur actif (CLAUDE.md §17).</summary>
    public async Task<UserDto?> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }
        if (entity.Statut == ReferentialStatus.Inactif)
        {
            throw new BusinessConflictException("Cet utilisateur est déjà désactivé.");
        }

        await EnsureNotLastActiveAdministratorAsync(entity, cancellationToken);

        entity.Statut = ReferentialStatus.Inactif;
        entity.DateSortie ??= DateOnly.FromDateTime(DateTime.UtcNow);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.StatusChange, nameof(User), id,
            new { Statut = ReferentialStatus.Actif }, new { entity.Statut }, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    public async Task<UserDto?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }
        if (entity.Statut == ReferentialStatus.Actif)
        {
            throw new BusinessConflictException("Cet utilisateur est déjà actif.");
        }

        entity.Statut = ReferentialStatus.Actif;
        entity.DateSortie = null;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.StatusChange, nameof(User), id,
            new { Statut = ReferentialStatus.Inactif }, new { entity.Statut }, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    /// <summary>§28.3 "changement de rôle" / "promotion ou retrait Administrateur". Le motif est
    /// conservé dans l'audit, jamais sur l'entité (pas de champ dédié sur <see cref="User"/>).</summary>
    public async Task<UserDto?> ChangeRoleAsync(Guid id, RoleChangeRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var newRoleExists = await roleRepository.Query().AnyAsync(r => r.Id == request.RoleId, cancellationToken);
        if (!newRoleExists)
        {
            throw new BusinessConflictException("Le rôle indiqué n'existe pas.");
        }

        var wasAdministrateur = await IsAdministrateurRoleAsync(entity.RoleId, cancellationToken);
        var willBeAdministrateur = await IsAdministrateurRoleAsync(request.RoleId, cancellationToken);
        if (wasAdministrateur && !willBeAdministrateur)
        {
            await EnsureNotLastActiveAdministratorAsync(entity, cancellationToken);
        }

        var oldRoleId = entity.RoleId;
        var now = DateTime.UtcNow;
        entity.RoleId = request.RoleId;
        entity.SecurityLastModifiedAt = now;
        entity.SecurityLastModifiedBy = currentUser.Identifier;
        entity.UpdatedAt = now;
        entity.UpdatedBy = currentUser.Identifier;

        var action = willBeAdministrateur && !wasAdministrateur
            ? AuditActions.AdminGranted
            : wasAdministrateur && !willBeAdministrateur
                ? AuditActions.AdminRevoked
                : AuditActions.RoleChange;

        await auditService.RecordAsync(
            action, nameof(User), id, new { RoleId = oldRoleId }, new { entity.RoleId }, request.Motif, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    /// <summary>§28.3 "changement de permission" (modèle RBAC, Lot 13) : octroie une exception
    /// individuelle. Si le rôle de l'utilisateur accorde déjà effectivement cette permission (via
    /// <see cref="RolePermission"/> ou un octroi individuel existant), l'action est un conflit — la
    /// notion de "déjà accordée" se lit désormais sur le résultat effectif
    /// (<see cref="PermissionResolutionService"/>), pas sur la seule existence d'une ligne
    /// individuelle. Si une ligne de retrait individuel existe pour cette permission, elle est
    /// remplacée par un octroi explicite (garantit l'accès quel que soit le rôle).</summary>
    public async Task<UserDto?> GrantPermissionAsync(Guid id, string permissionCode, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var permission = await permissionRepository.Query().FirstOrDefaultAsync(p => p.Code == permissionCode, cancellationToken)
            ?? throw new BusinessConflictException($"La permission '{permissionCode}' n'existe pas.");

        var effectiveCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(id, cancellationToken);
        var existingEffect = await userPermissionRepository.Query()
            .Where(up => up.UserId == id && up.PermissionId == permission.Id)
            .Select(up => (UserPermissionEffect?)up.Effect)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingEffect != UserPermissionEffect.Revoke
            && effectiveCodes.Contains(permissionCode, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessConflictException("Cette permission est déjà accordée à cet utilisateur.");
        }

        if (existingEffect is not null)
        {
            // Remplace la ligne existante (retrait individuel) par un octroi explicite : deux
            // instances EF Core distinctes ne peuvent pas être suivies simultanément pour la même
            // clé, on vide donc le suivi de la suppression avant d'ajouter la nouvelle ligne.
            await userPermissionRepository.RemoveAsync(new UserPermission { UserId = id, PermissionId = permission.Id }, cancellationToken);
            await userPermissionRepository.SaveChangesAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        await userPermissionRepository.AddAsync(
            new UserPermission { UserId = id, PermissionId = permission.Id, Effect = UserPermissionEffect.Grant, GrantedAt = now, GrantedBy = currentUser.Identifier },
            cancellationToken);

        entity.SecurityLastModifiedAt = now;
        entity.SecurityLastModifiedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.PermissionGranted, nameof(User), id, null, new { PermissionCode = permissionCode }, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    /// <summary>§28.3 "changement de permission" (modèle RBAC, Lot 13). Retrait d'une permission
    /// financière soumis au même garde-fou de principe que le rôle Administrateur (CLAUDE.md §17) :
    /// appliqué ici via la permission dédiée <c>USER_ADMINISTRATION</c> exigée côté contrôleur, pas
    /// de vérification de "dernier détenteur" supplémentaire (contrairement au rôle Administrateur,
    /// une permission complémentaire n'est jamais la seule porte d'entrée de l'application). Si le
    /// rôle accorde encore la permission après la suppression d'un éventuel octroi individuel, une
    /// ligne de retrait explicite est matérialisée — un simple retrait de l'octroi individuel ne
    /// suffit pas à retirer un accès qui provient du rôle.</summary>
    public async Task<UserDto?> RevokePermissionAsync(Guid id, string permissionCode, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var permission = await permissionRepository.Query().FirstOrDefaultAsync(p => p.Code == permissionCode, cancellationToken)
            ?? throw new BusinessConflictException($"La permission '{permissionCode}' n'existe pas.");

        var effectiveCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(id, cancellationToken);
        if (!effectiveCodes.Contains(permissionCode, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessConflictException("Cette permission n'est pas accordée à cet utilisateur.");
        }

        var rolePermissionCodes = await permissionResolutionService.GetRolePermissionCodesAsync(entity.RoleId, cancellationToken);
        var roleGrantsIt = rolePermissionCodes.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);

        var existingEffect = await userPermissionRepository.Query()
            .Where(up => up.UserId == id && up.PermissionId == permission.Id)
            .Select(up => (UserPermissionEffect?)up.Effect)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingEffect is not null)
        {
            await userPermissionRepository.RemoveAsync(new UserPermission { UserId = id, PermissionId = permission.Id }, cancellationToken);
            await userPermissionRepository.SaveChangesAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        if (roleGrantsIt)
        {
            await userPermissionRepository.AddAsync(
                new UserPermission { UserId = id, PermissionId = permission.Id, Effect = UserPermissionEffect.Revoke, GrantedAt = now, GrantedBy = currentUser.Identifier },
                cancellationToken);
        }

        entity.SecurityLastModifiedAt = now;
        entity.SecurityLastModifiedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.PermissionRevoked, nameof(User), id, new { PermissionCode = permissionCode }, null, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, cancellationToken);
    }

    private async Task<UserDto> ToDtoAsync(User entity, CancellationToken cancellationToken)
    {
        var dto = entity.Adapt<UserDto>();
        dto.EffectivePermissionCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(entity.Id, cancellationToken);
        return dto;
    }

    private async Task<bool> IsAdministrateurRoleAsync(Guid roleId, CancellationToken cancellationToken) =>
        await roleRepository.Query().AnyAsync(r => r.Id == roleId && r.Code == AdministrateurRoleCode, cancellationToken);

    /// <summary>CLAUDE.md §17 : "un utilisateur ne peut pas retirer son propre dernier accès
    /// administrateur si cela laisse l'application sans administrateur actif". Appliqué comme un
    /// invariant système (aucun autre Administrateur actif ne doit rester à zéro), pas seulement
    /// pour l'auto-retrait, pour ne jamais laisser l'application sans administrateur quel que soit
    /// l'auteur de l'action.</summary>
    private async Task EnsureNotLastActiveAdministratorAsync(User user, CancellationToken cancellationToken)
    {
        if (!await IsAdministrateurRoleAsync(user.RoleId, cancellationToken))
        {
            return;
        }

        var administrateurRoleId = await roleRepository.Query()
            .Where(r => r.Code == AdministrateurRoleCode)
            .Select(r => r.Id)
            .FirstAsync(cancellationToken);

        var otherActiveAdministrators = await repository.Query()
            .CountAsync(u => u.Id != user.Id && u.Statut == ReferentialStatus.Actif && u.RoleId == administrateurRoleId, cancellationToken);

        if (otherActiveAdministrators == 0)
        {
            throw new BusinessConflictException(
                "Impossible : cette action retirerait le dernier accès Administrateur actif de l'application (CLAUDE.md §17).");
        }
    }
}
