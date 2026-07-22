namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Exception individuelle à la matrice <see cref="RolePermission"/> pour un utilisateur donné
/// (cahier des charges §6.1, Lot 13) : <see cref="Effect"/> distingue un octroi complémentaire
/// (<see cref="UserPermissionEffect.Grant"/>, comportement historique des Lots 1/5/6) d'un retrait
/// explicite qui prime sur le rôle (<see cref="UserPermissionEffect.Revoke"/>). Au plus une ligne
/// par couple (UserId, PermissionId) : le calcul effectif vit dans
/// <c>PermissionResolutionService</c>, jamais dupliqué ici. Les modifications de sécurité sont
/// intégralement auditées (cahier des charges §6.4) : on conserve donc l'auteur et la date
/// d'attribution directement sur la relation.
/// </summary>
public class UserPermission
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public UserPermissionEffect Effect { get; set; } = UserPermissionEffect.Grant;
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
}
