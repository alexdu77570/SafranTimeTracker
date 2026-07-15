namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Permission complémentaire (cahier des charges §6.2, §30). Table de référence extensible :
/// FINANCIAL_DATA_VIEW est le minimum requis, d'autres permissions pourront être ajoutées
/// sans modification de code.
/// </summary>
public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
