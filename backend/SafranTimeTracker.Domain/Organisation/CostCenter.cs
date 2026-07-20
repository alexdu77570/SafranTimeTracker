using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Organisation;

/// <summary>
/// Référentiel des centres de coûts (docs/BACKLOG_METIER.md §8, Lot 8) : axe organisationnel
/// analytique rattaché à un Department et/ou un Service. Aucun impact sur le calcul financier
/// existant (FinancialCalculationService, BudgetService) — purement un attribut analytique.
/// </summary>
public class CostCenter : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
