using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Financial.Dtos;

/// <summary>Cahier des charges §11.2. Endpoint entièrement gardé par FINANCIAL_DATA_VIEW
/// (CLAUDE.md §12) : aucune omission de champ à la pièce, l'accès est total ou nul.</summary>
public class ResourceTjmHistoryDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public string? Reason { get; set; }
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ResourceTjmHistoryCreateRequest
{
    public Guid ResourceId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public string? Reason { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Sert à la fois à corriger une période (§10.2 : "modifier une période non utilisée") et à la
/// clôturer (renseigner EndDate). Reason obligatoire ici : c'est le seul champ de motif porté par
/// cette entité (§11.2) — voir docs/IMPLEMENTATION_STATUS.md pour la limite (pas d'AuditLog avant
/// le Lot 6, donc pas de conservation structurée de l'ancienne valeur au-delà de UpdatedAt/By).
/// </summary>
public class ResourceTjmHistoryUpdateRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; }
}
