namespace SafranTimeTracker.Domain.Reporting;

/// <summary>Catégorie d'un KPI de tableau de bord (cahier des charges §25.1 "KPI opérationnels"
/// vs §25.2 "KPI financiers" — ces derniers visibles uniquement avec FINANCIAL_DATA_VIEW).</summary>
public enum DashboardKpiCategory
{
    Operationnel,
    Financier
}
