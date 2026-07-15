namespace SafranTimeTracker.Domain.Resources;

/// <summary>
/// Rôle opérationnel (cahier des charges §10.4, §30) : RUN, Build, Amélioration continue,
/// Chef de Projet, Coordinateur IT. Indépendant du rôle applicatif. Une ressource peut en
/// cumuler plusieurs (voir <see cref="ResourceOperationalRole"/>).
/// </summary>
public class OperationalRole
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    public ICollection<ResourceOperationalRole> ResourceOperationalRoles { get; set; } = new List<ResourceOperationalRole>();
}
