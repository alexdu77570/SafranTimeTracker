using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Technologies.Dtos;

public class TechnologyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
    public IReadOnlyCollection<Guid> ApplicationIds { get; set; } = [];
    public IReadOnlyCollection<Guid> ResourceIds { get; set; } = [];
}

public class TechnologyCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public List<Guid> ApplicationIds { get; set; } = [];
    public List<Guid> ResourceIds { get; set; } = [];
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class TechnologyUpdateRequest
{
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
    public List<Guid> ApplicationIds { get; set; } = [];
    public List<Guid> ResourceIds { get; set; } = [];
}
