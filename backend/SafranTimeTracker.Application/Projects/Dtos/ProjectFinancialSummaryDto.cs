namespace SafranTimeTracker.Application.Projects.Dtos;

/// <summary>
/// Sous-objet financier isolé (cahier des charges §16.2, §17.1, §17.4 ; CLAUDE.md §13, §17 —
/// "budgets"/"montants" sont explicitement des données financières) : présent dans
/// ProjectDto.FinancialSummary uniquement si l'appelant a FINANCIAL_DATA_VIEW. Coûts consommés
/// agrégés à partir des TimeEntryFinancialSnapshot des saisies liées au projet (§20.6 : somme des
/// valeurs historisées, jamais un recalcul aux taux actuels).
/// </summary>
public class ProjectFinancialSummaryDto
{
    public decimal? BudgetInitial { get; set; }
    public decimal CoutReelConsomme { get; set; }
    public decimal CoutContractuelConsomme { get; set; }
    public decimal Differentiel { get; set; }

    /// <summary>Null si BudgetInitial n'est pas renseigné.</summary>
    public decimal? BudgetRestant { get; set; }
}
