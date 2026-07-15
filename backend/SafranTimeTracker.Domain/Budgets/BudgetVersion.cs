namespace SafranTimeTracker.Domain.Budgets;

/// <summary>
/// Historique d'ajustement d'un budget (cahier des charges §14.2) : ancienne valeur, nouvelle
/// valeur, motif, auteur, date, pièce de référence textuelle éventuelle ("sans dépôt
/// documentaire" — un simple texte, jamais un fichier, cahier des charges §14.2). Entité
/// append-only, même principe que <see cref="Orders.OrderExtension"/> : jamais corrigée ni
/// supprimée.
/// </summary>
public class BudgetVersion
{
    public Guid Id { get; set; }

    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;

    public decimal OldValue { get; set; }
    public decimal NewValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferencePiece { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
