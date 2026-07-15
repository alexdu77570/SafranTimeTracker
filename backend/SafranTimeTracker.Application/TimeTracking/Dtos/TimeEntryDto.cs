using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.TimeTracking.Dtos;

public class TimeEntryDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid ActivityTypeId { get; set; }
    public Guid? OrderId { get; set; }
    public DateOnly Date { get; set; }
    public decimal DureeHeures { get; set; }
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
    public ReferentialStatus Statut { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    /// <summary>Null si l'appelant n'a pas FINANCIAL_DATA_VIEW (projection côté service,
    /// CLAUDE.md §13) — jamais un filtrage a posteriori côté client.</summary>
    public TimeEntryFinancialSnapshotDto? FinancialSnapshot { get; set; }
}

public class TimeEntryCreateRequest
{
    public Guid ResourceId { get; set; }
    public Guid ActivityTypeId { get; set; }
    public Guid? OrderId { get; set; }
    public DateOnly Date { get; set; }
    public decimal DureeHeures { get; set; }
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
}

/// <summary>Une modification autorisée revalorise la saisie (§19.5) : ResourceId n'est volontairement
/// pas modifiable (changer le titulaire d'une saisie est une opération différente — annuler puis
/// recréer).</summary>
public class TimeEntryUpdateRequest
{
    public Guid ActivityTypeId { get; set; }
    public Guid? OrderId { get; set; }
    public DateOnly Date { get; set; }
    public decimal DureeHeures { get; set; }
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
}
