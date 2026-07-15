using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Milestones;

/// <summary>Jalon (cahier des charges §24).</summary>
public class Milestone : AuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public Guid MilestoneTypeId { get; set; }
    public MilestoneType MilestoneType { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? ApplicationId { get; set; }
    public ApplicationReference? Application { get; set; }

    public Guid ResponsableId { get; set; }
    public Resource Responsable { get; set; } = null!;

    public DateOnly DatePrevue { get; set; }
    public DateOnly? DateReelle { get; set; }
    public MilestoneStatus Statut { get; set; } = MilestoneStatus.AVenir;
    public MilestoneCriticality Criticite { get; set; } = MilestoneCriticality.Moyenne;
    public string? Commentaire { get; set; }

    /// <summary>Dépendance éventuelle (§24.2) : simple référence, aucune détection de cycle
    /// (non demandée par le cahier des charges).</summary>
    public Guid? DependsOnMilestoneId { get; set; }
    public Milestone? DependsOnMilestone { get; set; }
}
