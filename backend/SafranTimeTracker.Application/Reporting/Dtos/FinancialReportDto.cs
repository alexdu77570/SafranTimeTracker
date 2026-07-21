namespace SafranTimeTracker.Application.Reporting.Dtos;

public class FinancialReportDifferentialDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal CoutReel { get; set; }
    public decimal CoutContractuel { get; set; }
    public decimal Differentiel { get; set; }
}

public class FinancialReportOrderAlertDto
{
    public Guid OrderId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal BudgetFinancierAjuste { get; set; }
    public decimal CoutReelConsomme { get; set; }
    public DateOnly? DateFinAjustee { get; set; }
}

/// <summary>Cahier des charges §14.3 : "consommation mensuelle" de la page Budgets. Un mois n'est
/// présent que s'il porte au moins une saisie valorisée sur la période demandée (pas de mois à
/// zéro généré artificiellement).</summary>
public class FinancialReportMonthlyConsumptionDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public decimal CoutReel { get; set; }
    public decimal CoutContractuel { get; set; }
    public decimal Differentiel { get; set; }
}

/// <summary>
/// Cahier des charges §26.2 : ressource intégralement financière, gardée par FINANCIAL_DATA_VIEW
/// au niveau contrôleur (même principe que BudgetDto). "Commandes à renouveler" (fenêtre fixe de 30
/// jours avant échéance, faute de paramètre dédié au §28.2) et "besoins de rallonge" (reste financier
/// négatif ou sous le seuil d'alerte de la commande) sont des simplifications documentées.
/// </summary>
public class FinancialReportDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }

    public decimal DifferentielGlobal { get; set; }
    public decimal BudgetRestant { get; set; }
    public decimal AtterrissageEstime { get; set; }

    public IReadOnlyList<FinancialReportDifferentialDto> DifferentielParProjet { get; set; } = [];
    public IReadOnlyList<FinancialReportDifferentialDto> DifferentielParCommande { get; set; } = [];
    public IReadOnlyList<FinancialReportDifferentialDto> DifferentielParSociete { get; set; } = [];
    public IReadOnlyList<FinancialReportDifferentialDto> DifferentielParRessource { get; set; } = [];
    public IReadOnlyList<FinancialReportMonthlyConsumptionDto> ConsommationMensuelle { get; set; } = [];

    public IReadOnlyList<FinancialReportOrderAlertDto> BesoinsRallonge { get; set; } = [];
    public IReadOnlyList<FinancialReportOrderAlertDto> CommandesARenouveler { get; set; } = [];
    public IReadOnlyList<string> SourcesMontants { get; set; } = [];
}
