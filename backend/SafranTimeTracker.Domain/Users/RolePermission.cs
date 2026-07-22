namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Permission accordée par défaut à un rôle applicatif (modèle RBAC, cahier des charges §6.1,
/// Lot 13) : le calcul des permissions effectives d'un utilisateur part de la matrice portée par
/// cette table avant d'appliquer les exceptions individuelles (<see cref="UserPermission"/>). Table
/// de référence simple (pas d'AuditableEntity), gérée par un administrateur au même titre que
/// <see cref="Role"/>/<see cref="Permission"/>.
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
