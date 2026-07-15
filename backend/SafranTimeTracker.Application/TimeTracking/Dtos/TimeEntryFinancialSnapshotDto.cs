using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.TimeTracking.Dtos;

/// <summary>
/// Sous-objet financier isolé (cahier des charges §19.5, CLAUDE.md §13) : présent dans
/// TimeEntryDto.FinancialSnapshot uniquement si l'appelant a FINANCIAL_DATA_VIEW, absent sinon —
/// jamais un champ à `null` masqué côté client (CLAUDE.md §12).
/// </summary>
public class TimeEntryFinancialSnapshotDto
{
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
