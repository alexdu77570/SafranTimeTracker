using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Financial.Dtos;

/// <summary>Cahier des charges §12.3. Données confidentielles, endpoint entièrement gardé par
/// FINANCIAL_DATA_VIEW (§12.4, CLAUDE.md §12).</summary>
public class CompanyContractHistoryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string? ContractNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractDailyRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class CompanyContractHistoryCreateRequest
{
    public Guid CompanyId { get; set; }
    public string? ContractNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractDailyRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

/// <summary>Corrige ou clôture une période (§12.4). Comment obligatoire ici : cette entité ne
/// porte pas de champ "Reason" dédié (§12.3) — voir docs/IMPLEMENTATION_STATUS.md pour la limite
/// (pas d'AuditLog avant le Lot 6).</summary>
public class CompanyContractHistoryUpdateRequest
{
    public string? ContractNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractDailyRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public ReferentialStatus Status { get; set; }
}
