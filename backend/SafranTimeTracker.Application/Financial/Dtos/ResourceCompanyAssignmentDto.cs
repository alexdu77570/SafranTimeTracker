using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Financial.Dtos;

/// <summary>Cahier des charges §12.2. Source de vérité historisée utilisée par
/// FinancialCalculationService (docs/DATABASE.md §5) — distincte de Resource.CompanyId (Lot 1),
/// pointeur d'affichage non historisé.</summary>
public class ResourceCompanyAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid CompanyId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public class ResourceCompanyAssignmentCreateRequest
{
    public Guid ResourceId { get; set; }
    public Guid CompanyId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

/// <summary>Corrige ou clôture un rattachement (§12.2). Comment obligatoire ici : cette entité ne
/// porte pas de champ "Reason" dédié — voir docs/IMPLEMENTATION_STATUS.md pour la limite (pas
/// d'AuditLog avant le Lot 6).</summary>
public class ResourceCompanyAssignmentUpdateRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public ReferentialStatus Status { get; set; }
}
