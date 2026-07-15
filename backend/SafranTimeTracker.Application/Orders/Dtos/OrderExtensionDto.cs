namespace SafranTimeTracker.Application.Orders.Dtos;

public class OrderExtensionDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
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

/// <summary>ExtensionDate, PreviousEndDate sont dérivés côté service (date du jour, date de fin
/// ajustée actuelle de la commande) — seuls le montant ajouté, la nouvelle date de fin et le motif
/// sont saisis (cahier des charges §13.3).</summary>
public class OrderExtensionCreateRequest
{
    public decimal AmountAdded { get; set; }
    public decimal? DaysAdded { get; set; }
    public DateOnly NewEndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
