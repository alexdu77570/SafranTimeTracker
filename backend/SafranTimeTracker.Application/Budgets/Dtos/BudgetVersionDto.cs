namespace SafranTimeTracker.Application.Budgets.Dtos;

public class BudgetVersionDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public decimal OldValue { get; set; }
    public decimal NewValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferencePiece { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>Ajustement du montant ajusté du budget (cahier des charges §14.2) : OldValue est
/// dérivé côté service (AdjustedAmount courant), seul NewValue est saisi.</summary>
public class BudgetAdjustRequest
{
    public decimal NewValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferencePiece { get; set; }
}
