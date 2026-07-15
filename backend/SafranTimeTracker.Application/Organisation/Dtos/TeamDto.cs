using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Organisation.Dtos;

public class TeamDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Guid? ResponsableFonctionnelId { get; set; }
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
}

public class TeamCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Guid? ResponsableFonctionnelId { get; set; }
    public string? Commentaire { get; set; }
}
