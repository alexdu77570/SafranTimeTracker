using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Version de planning (cahier des charges §18.3). Une seule version Initiale par projet
/// (immuable). Plusieurs versions Ajustées possibles dans le temps ; une seule Active à la fois
/// (voir <see cref="ProjectPlanVersionStatus"/>), jamais supprimée (CLAUDE.md §7). "Date de
/// création" et "auteur" (§18.3) ne sont pas des champs distincts : couverts par
/// <c>CreatedAt</c>/<c>CreatedBy</c> (AuditableEntity). Le réalisé provient des saisies de temps
/// (<c>TimeEntry.ProjectId</c>) et n'est jamais une troisième version saisie manuellement (§18.3).
/// </summary>
public class ProjectPlanVersion : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ProjectPlanVersionType Type { get; set; }
    public ProjectPlanVersionStatus Statut { get; set; } = ProjectPlanVersionStatus.Active;
    public string? Motif { get; set; }

    public ICollection<ProjectWeeklyPlan> WeeklyPlans { get; set; } = new List<ProjectWeeklyPlan>();
}
