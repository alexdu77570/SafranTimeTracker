using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Time;

/// <summary>
/// Instantané financier figé d'une saisie de temps (cahier des charges §19.5, §20.5,
/// docs/DATABASE.md §5). Relation 1-1 avec TimeEntry. Produit exclusivement par
/// FinancialCalculationService (Lot 2, seul point de calcul, docs/ARCHITECTURE.md §2) — jamais
/// recalculé silencieusement (§4.3) : seule TimeEntryService (création/modification autorisée,
/// §19.5) écrit ces valeurs ; un recalcul a posteriori (§19.6) est différé au Lot 6
/// (ITimeEntryRevaluationService).
/// </summary>
public class TimeEntryFinancialSnapshot : AuditableEntity
{
    public Guid TimeEntryId { get; set; }
    public TimeEntry TimeEntry { get; set; } = null!;

    public decimal? TjmPersonneSnapshot { get; set; }
    public string? SourceTjmPersonne { get; set; }
    public Guid? ResourceTjmHistoryId { get; set; }

    public decimal? TjmContratSnapshot { get; set; }
    public string? SourceContrat { get; set; }
    public Guid? CompanyContractHistoryId { get; set; }
    public Guid? CompanyIdSnapshot { get; set; }

    public decimal? CoutReelCalcule { get; set; }
    public decimal? CoutContratCalcule { get; set; }
    public decimal? DifferentielCalcule { get; set; }

    public DateTime CalculationDate { get; set; }
    public FinancialValuationStatus CalculationStatus { get; set; }
}
