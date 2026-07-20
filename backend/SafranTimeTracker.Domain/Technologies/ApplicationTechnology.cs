using SafranTimeTracker.Domain.Applications;

namespace SafranTimeTracker.Domain.Technologies;

/// <summary>Rattachement d'une technologie à une application (docs/BACKLOG_METIER.md §5).</summary>
public class ApplicationTechnology
{
    public Guid ApplicationId { get; set; }
    public ApplicationReference Application { get; set; } = null!;
    public Guid TechnologyId { get; set; }
    public Technology Technology { get; set; } = null!;
}
