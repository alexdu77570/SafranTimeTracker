namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Attribution d'une permission complémentaire à un utilisateur. Les modifications de sécurité
/// sont intégralement auditées (cahier des charges §6.4) : on conserve donc l'auteur et la date
/// d'attribution directement sur la relation.
/// </summary>
public class UserPermission
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
}
