using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.TimeTracking.Dtos;

public class ActivityTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public bool IsRun { get; set; }
    public bool ReferenceRequired { get; set; }
    public string? ReferenceFormatRegex { get; set; }
    public string? ReferenceExample { get; set; }
    public ReferentialStatus Statut { get; set; }
}

public class ActivityTypeCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public bool IsRun { get; set; }
    public bool ReferenceRequired { get; set; }
    public string? ReferenceFormatRegex { get; set; }
    public string? ReferenceExample { get; set; }
}
