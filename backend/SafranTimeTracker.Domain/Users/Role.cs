namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Rôle applicatif (cahier des charges §5.2, §30). Modélisé comme une table de référence
/// (et non un enum C#) car le cahier des charges le nomme explicitement comme une entité
/// distincte. Table de référence simple (pas d'AuditableEntity) : données peu volatiles,
/// gérées par un administrateur.
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public int Ordre { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
