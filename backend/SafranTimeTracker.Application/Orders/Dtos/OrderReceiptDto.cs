namespace SafranTimeTracker.Application.Orders.Dtos;

/// <summary>Réception partielle d'une commande (règle métier validée Lot 6, voir
/// <c>Domain.Orders.OrderReceipt</c>). Append-only : aucun DTO de mise à jour, une correction se
/// fait par une nouvelle réception (éventuellement à montant/jours négatifs, explicitement
/// tracée).</summary>
public class OrderReceiptDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public decimal? ReceivedAmount { get; set; }
    public decimal? ReceivedDays { get; set; }
    public string? Reason { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>Exactement l'un des deux (ReceivedAmount/ReceivedDays) doit être renseigné, jamais les
/// deux (une commande est pilotée en montant OU en jours pour une réception donnée). Une valeur
/// négative est une écriture compensatoire explicite (correction d'une réception antérieure).</summary>
public class OrderReceiptCreateRequest
{
    public DateOnly ReceiptDate { get; set; }
    public decimal? ReceivedAmount { get; set; }
    public decimal? ReceivedDays { get; set; }
    public string? Reason { get; set; }
    public string? Comment { get; set; }
}

/// <summary>Le total réceptionné n'est jamais stocké (règle métier validée) : toujours recalculé
/// par somme des OrderReceipt de la commande, exposé ici pour éviter à chaque appelant de
/// recalculer la même agrégation.</summary>
public class OrderReceiptSummaryDto
{
    public decimal TotalReceivedAmount { get; set; }
    public decimal TotalReceivedDays { get; set; }
    public decimal RemainingReceivableAmount { get; set; }
    public decimal? RemainingReceivableDays { get; set; }
}
