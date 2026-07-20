using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Technologies;

/// <summary>Rattachement d'une technologie à une ressource (docs/BACKLOG_METIER.md §5).</summary>
public class ResourceTechnology
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    public Guid TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
}
