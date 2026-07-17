namespace SafranTimeTracker.Domain.Orders;

/// <summary>
/// Réception partielle d'une commande (règle métier validée Lot 6) : le vocabulaire "Demande
/// d'achat → Commande → Réceptions partielles → Clôture" se représente techniquement par la
/// machine d'état existante de <see cref="Order"/> (§13.2, inchangée depuis le Lot 5 — Brouillon
/// ≈ Demande d'achat, Active ≈ Commande, Clôturée ≈ Clôture) complétée par des événements
/// <see cref="OrderReceipt"/> répétables représentant les réceptions ("Réceptions partielles").
/// Entité append-only (même principe que <see cref="OrderExtension"/>/<c>BudgetVersion</c>) :
/// une réception n'est jamais corrigée ni supprimée, une erreur se compense par une nouvelle
/// réception explicitement tracée. Le total réceptionné n'est jamais stocké : toujours recalculé
/// par somme des <see cref="OrderReceipt"/> de la commande (voir <c>OrderReceiptService</c>).
/// </summary>
public class OrderReceipt
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public DateOnly ReceiptDate { get; set; }

    /// <summary>L'un des deux est renseigné selon que la commande est pilotée en montant ou en
    /// jours (§13.2 : BudgetFinancier et/ou BudgetJours) ; jamais aucun des deux à la fois.</summary>
    public decimal? ReceivedAmount { get; set; }
    public decimal? ReceivedDays { get; set; }

    public string? Reason { get; set; }
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
