namespace SafranTimeTracker.Application.Reporting.Dtos;

/// <summary>Cahier des charges §25.1 : toujours renvoyés, aucune donnée financière.</summary>
public class DashboardOperationalKpisDto
{
    public decimal TempsSaisisHeures { get; set; }
    public decimal CapaciteTheorique { get; set; }
    public decimal CapaciteReelle { get; set; }
    public decimal TauxDisponibilite { get; set; }
    public decimal ChargeRunHeures { get; set; }
    public decimal ChargeHorsRunHeures { get; set; }
    public int IncidentsOuverts { get; set; }
    public int ChangesEnCours { get; set; }
    public int ProblemsOuverts { get; set; }
    public int RitmEnCours { get; set; }
    public int ProjetsActifs { get; set; }
    public int JalonsEnRetard { get; set; }
    public int RessourcesSurchargees { get; set; }
    public int RessourcesSousChargees { get; set; }
}

/// <summary>Cahier des charges §25.2 : visible uniquement avec FINANCIAL_DATA_VIEW. Atterrissage
/// estimé = coût réel total consommé (simplification MVP validée, cf. ProjectPlanningCalculator/
/// BudgetService : pas de modèle prédictif avancé faute de plan de charge global).</summary>
public class DashboardFinancialKpisDto
{
    public decimal BudgetInitialTotal { get; set; }
    public decimal BudgetAjusteTotal { get; set; }
    public decimal CoutReelTotal { get; set; }
    public decimal CoutContractuelTotal { get; set; }
    public decimal DifferentielGlobal { get; set; }
    public decimal BudgetRestant { get; set; }
    public int CommandesARisque { get; set; }
    public int ProjetsSousFinances { get; set; }
    public decimal AtterrissageEstime { get; set; }
}

public class DashboardDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public DashboardOperationalKpisDto Operational { get; set; } = new();

    /// <summary>Null sans FINANCIAL_DATA_VIEW (CLAUDE.md §13).</summary>
    public DashboardFinancialKpisDto? Financial { get; set; }
}
