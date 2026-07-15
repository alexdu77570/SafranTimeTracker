namespace SafranTimeTracker.Domain.Resources;

/// <summary>Jointure many-to-many : une ressource peut cumuler plusieurs rôles opérationnels (§10.4).</summary>
public class ResourceOperationalRole
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    public Guid OperationalRoleId { get; set; }
    public OperationalRole OperationalRole { get; set; } = null!;
}
