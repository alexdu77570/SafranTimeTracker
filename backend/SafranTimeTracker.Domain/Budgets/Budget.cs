using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Domain.Budgets;

/// <summary>
/// Enveloppe de suivi budgétaire liée à un projet et/ou une commande (cahier des charges §14.1).
/// Distincte du budget propre à <see cref="Order"/> (§13.2, augmenté par <see cref="OrderExtension"/>) :
/// <c>Budget</c> est l'objet de pilotage transverse que le §14 décrit, avec son propre historique
/// d'ajustement (<see cref="BudgetVersion"/>). Le "périmètre de pilotage" libre évoqué au §14.1
/// (hors projet/commande) n'est pas modélisé ce lot, faute d'entité "périmètre" dédiée au §30 —
/// simplification documentée (voir docs/IMPLEMENTATION_STATUS.md) : <see cref="ProjectId"/> et/ou
/// <see cref="OrderId"/> doivent porter au moins une valeur (FluentValidation, pas de contrainte
/// SQL — CLAUDE.md §11 : pas de règle complexe en annotation). Consommé/reste/atterrissage ne sont
/// jamais des colonnes : <c>BudgetService</c> les calcule à la demande depuis
/// <see cref="Domain.Time.TimeEntryFinancialSnapshot"/>, même principe que <see cref="Project"/>.
/// </summary>
public class Budget : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public decimal InitialAmount { get; set; }
    public decimal AdjustedAmount { get; set; }

    public BudgetStatus Status { get; set; } = BudgetStatus.Actif;
    public decimal? AlertThreshold { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Comment { get; set; }

    public ICollection<BudgetVersion> Versions { get; set; } = new List<BudgetVersion>();
}
