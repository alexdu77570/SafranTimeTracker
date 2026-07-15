namespace SafranTimeTracker.Domain.Common;

/// <summary>
/// Statut de valorisation d'un calcul financier (cahier des charges §11.4). "Incomplete" signifie
/// qu'aucun TJM valide n'a été trouvé à la date demandée : aucun montant n'est inventé, le calcul
/// remonte ce statut plutôt qu'une valeur silencieusement fausse.
/// </summary>
public enum FinancialValuationStatus
{
    Complete,
    Incomplete
}
