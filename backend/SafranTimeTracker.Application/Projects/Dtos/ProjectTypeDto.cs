using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Projects.Dtos;

public class ProjectTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}

public class ProjectTypeCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class ProjectTypeUpdateRequest
{
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}
