using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Financial.Dtos;

/// <summary>Entrée du calcul unique (cahier des charges §20.1) : la conversion heures → jours
/// utilise Settings.HeuresParJour, jamais une valeur codée en dur.</summary>
public class FinancialCalculationRequest
{
    public Guid ResourceId { get; set; }
    public DateOnly Date { get; set; }
    public decimal HeuresSaisies { get; set; }
}

/// <summary>
/// Résultat du calcul financier unique (FinancialCalculationService). Ce n'est pas l'entité
/// persistée TimeEntryFinancialSnapshot (docs/DATABASE.md §5) : celle-ci référence TimeEntry, qui
/// n'existe qu'au Lot 3 — la persistance du snapshot est différée à ce lot. Ce DTO couvre le
/// sous-ensemble testable indépendamment d'une saisie (§20.2-20.4).
/// </summary>
public class FinancialCalculationResultDto
{
    public Guid ResourceId { get; set; }
    public DateOnly Date { get; set; }
    public decimal HeuresSaisies { get; set; }
    public decimal HeuresParJour { get; set; }
    public decimal TempsJours { get; set; }

    public Guid? ResourceTjmHistoryId { get; set; }
    public decimal? DailyRatePersonne { get; set; }
    public decimal? CoutReel { get; set; }

    public Guid? CompanyId { get; set; }
    public Guid? CompanyContractHistoryId { get; set; }
    public decimal? DailyRateContrat { get; set; }
    public decimal? CoutContractuel { get; set; }

    /// <summary>coutContractuel - coutReel (§20.4). Null si non applicable (société interne ou
    /// aucun contrat valide à la date) ou si le coût réel lui-même est incalculable.</summary>
    public decimal? Differentiel { get; set; }

    /// <summary>Reflète uniquement l'absence de TJM personne valide à la date (§11.4) : un contrat
    /// manquant ou une société interne ne rendent jamais ce statut Incomplete, ils rendent
    /// seulement CoutContractuel/Differentiel non applicables (null).</summary>
    public FinancialValuationStatus ValuationStatus { get; set; }
}
