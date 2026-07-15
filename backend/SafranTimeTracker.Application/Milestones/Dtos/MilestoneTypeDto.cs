using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Milestones.Dtos;

public class MilestoneTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}

public class MilestoneTypeCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
}
