using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Domain.Orders;

/// <summary>
/// Enveloppe contractuelle ou budgétaire consommable (cahier des charges §13). Ne porte en
/// Lot 1 que les champs administratifs :
/// - le lien vers Project est différé au Lot 4 (Project n'existe pas encore) ;
/// - budget ajusté, dates ajustées et rallonges (OrderExtension) sont différés au Lot 5 (§13.3) ;
/// - consommation en jours, coût réel/contractuel consommé, différentiel et restes sont des
///   valeurs calculées à partir des saisies de temps valorisées (Lot 2/3), donc absentes ici.
/// </summary>
public class Order : AuditableEntity
{
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public decimal BudgetFinancierInitial { get; set; }
    public decimal? BudgetJoursInitial { get; set; }

    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }

    public Guid StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;

    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }

    public ICollection<OrderAuthorizedResource> AuthorizedResources { get; set; } = new List<OrderAuthorizedResource>();
}
