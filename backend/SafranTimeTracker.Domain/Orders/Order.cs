using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Domain.Orders;

/// <summary>
/// Enveloppe contractuelle ou budgétaire consommable (cahier des charges §13). Budget ajusté,
/// date de fin ajustée et rallonges (<see cref="OrderExtension"/>) portés depuis le Lot 5 (§13.3) ;
/// le lien vers <see cref="Project"/> (§13.2 "projet lié facultatif") est également raccordé ce
/// lot. Consommation en jours, coût réel/contractuel consommé, différentiel et restes restent des
/// valeurs calculées à la demande à partir des saisies de temps valorisées (Lot 2/3/5), jamais des
/// colonnes stockées.
/// </summary>
public class Order : AuditableEntity
{
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    public decimal BudgetFinancierInitial { get; set; }
    public decimal BudgetFinancierAjuste { get; set; }
    public decimal? BudgetJoursInitial { get; set; }
    public decimal? BudgetJoursAjuste { get; set; }

    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }
    public DateOnly? DateFinAjustee { get; set; }

    public Guid StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;

    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }

    public ICollection<OrderAuthorizedResource> AuthorizedResources { get; set; } = new List<OrderAuthorizedResource>();
    public ICollection<OrderExtension> Extensions { get; set; } = new List<OrderExtension>();
}
