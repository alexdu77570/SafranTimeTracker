using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Ligne de planning hebdomadaire par ressource (cahier des charges §17.3 : "les charges peuvent
/// varier chaque semaine", §18.2-18.3). <see cref="WeekStartDate"/> est le lundi de la semaine
/// concernée. Exprimée en heures pour rester directement comparable aux saisies de temps réelles
/// (<c>TimeEntry.DureeHeures</c>) dans le calcul des écarts (§29.5).
/// </summary>
public class ProjectWeeklyPlan : AuditableEntity
{
    public Guid ProjectPlanVersionId { get; set; }
    public ProjectPlanVersion ProjectPlanVersion { get; set; } = null!;

    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public DateOnly WeekStartDate { get; set; }
    public decimal ChargePlanifieeHeures { get; set; }
}
