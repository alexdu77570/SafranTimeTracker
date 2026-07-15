namespace SafranTimeTracker.Domain.Budgets;

/// <summary>
/// Statut d'un budget (cahier des charges §14.1 "status", valeurs non fixées par le cahier).
/// Deux valeurs suffisent au périmètre du Lot 5 : un budget suivi activement, ou clôturé (plus
/// aucun ajustement possible, cohérent avec la clôture d'une commande, §13.2).
/// </summary>
public enum BudgetStatus
{
    Actif,
    Cloture
}
