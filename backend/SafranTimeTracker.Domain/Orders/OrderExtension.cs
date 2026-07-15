namespace SafranTimeTracker.Domain.Orders;

/// <summary>
/// Rallonge de commande (cahier des charges §13.3) : augmente le budget ajusté, conserve le budget
/// initial, reste visible dans l'historique. Entité append-only — aucune propriété <c>UpdatedAt</c>/
/// <c>UpdatedBy</c> : une rallonge appliquée n'est jamais corrigée ni supprimée (CLAUDE.md §7),
/// seule une nouvelle rallonge peut compenser une erreur.
/// </summary>
public class OrderExtension
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public DateOnly ExtensionDate { get; set; }
    public decimal AmountAdded { get; set; }
    public decimal? DaysAdded { get; set; }
    public DateOnly PreviousEndDate { get; set; }
    public DateOnly NewEndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
