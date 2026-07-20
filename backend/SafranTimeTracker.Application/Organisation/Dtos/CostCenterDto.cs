using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Organisation.Dtos;

public class CostCenterDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? ServiceId { get; set; }
    public ReferentialStatus Statut { get; set; }
}

public class CostCenterCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? ServiceId { get; set; }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class CostCenterUpdateRequest
{
    public string Libelle { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? ServiceId { get; set; }
    public ReferentialStatus Statut { get; set; }
}
