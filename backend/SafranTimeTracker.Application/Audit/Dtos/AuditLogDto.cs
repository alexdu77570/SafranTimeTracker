namespace SafranTimeTracker.Application.Audit.Dtos;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public string? TechnicalContext { get; set; }
}
