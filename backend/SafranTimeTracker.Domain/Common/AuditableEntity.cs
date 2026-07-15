namespace SafranTimeTracker.Domain.Common;

/// <summary>
/// Socle commun à toute entité historisant sa création/modification (docs/DATABASE.md §3).
/// Les dates sont toujours stockées en UTC (CLAUDE.md §5).
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
