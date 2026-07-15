using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Application.Milestones.Dtos;

public class MilestoneDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public Guid MilestoneTypeId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid ResponsableId { get; set; }
    public DateOnly DatePrevue { get; set; }
    public DateOnly? DateReelle { get; set; }
    public MilestoneStatus Statut { get; set; }
    public MilestoneCriticality Criticite { get; set; }
    public string? Commentaire { get; set; }
    public Guid? DependsOnMilestoneId { get; set; }

    /// <summary>État dérivé (§24.2), pas stocké : DatePrevue dépassée et statut ni Terminé ni
    /// Annulé (MilestoneService.ToDto).</summary>
    public bool EstEnRetard { get; set; }
}

public class MilestoneCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public Guid MilestoneTypeId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid ResponsableId { get; set; }
    public DateOnly DatePrevue { get; set; }
    public MilestoneCriticality Criticite { get; set; } = MilestoneCriticality.Moyenne;
    public string? Commentaire { get; set; }
    public Guid? DependsOnMilestoneId { get; set; }
}

/// <summary>Permet de renseigner la date réelle et de faire évoluer le statut (§24.2) — pas le
/// type ni le projet, qui définissent l'identité du jalon.</summary>
public class MilestoneUpdateRequest
{
    public string Nom { get; set; } = string.Empty;
    public Guid ResponsableId { get; set; }
    public DateOnly DatePrevue { get; set; }
    public DateOnly? DateReelle { get; set; }
    public MilestoneStatus Statut { get; set; }
    public MilestoneCriticality Criticite { get; set; }
    public string? Commentaire { get; set; }
    public Guid? DependsOnMilestoneId { get; set; }
}
