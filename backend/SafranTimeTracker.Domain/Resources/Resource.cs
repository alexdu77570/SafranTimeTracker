using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Domain.Resources;

/// <summary>
/// Personne planifiable et valorisable (cahier des charges §10.1). Porte sa propre identité
/// (Nom/Prénom) car une ressource peut exister sans compte utilisateur actif (§10.1) — voir
/// <see cref="Users.User.ResourceId"/> pour le sens du lien avec le compte.
///
/// Le TJM (ResourceTjmHistory) est explicitement hors périmètre du Lot 1 (Lot 2). De même,
/// les variations de capacité historisées (ResourceCapacityPeriod) sont différées au Lot 3 :
/// seule la capacité par défaut est portée ici.
/// </summary>
public class Resource : AuditableEntity
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? ResponsableHierarchiqueId { get; set; }
    public Resource? ResponsableHierarchique { get; set; }

    /// <summary>Société courante : pointeur simple non historisé (le rattachement historisé,
    /// ResourceCompanyAssignment, est différé au Lot 2 — voir docs/DATABASE.md §5).</summary>
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    /// <summary>Commande par défaut (§10.2).</summary>
    public Guid? DefaultOrderId { get; set; }
    public Order? DefaultOrder { get; set; }

    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }

    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }

    public ICollection<ResourceOperationalRole> OperationalRoles { get; set; } = new List<ResourceOperationalRole>();
}
