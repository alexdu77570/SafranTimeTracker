using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Projet (cahier des charges §16.2). Les charges (initiale/ajustée/consommée/restante) et les
/// montants consommés (coût réel/contractuel/différentiel, budget restant) ne sont pas des
/// colonnes : <c>ProjectPlanningService</c> les calcule à la demande à partir de
/// <see cref="ProjectPlanVersion"/>/<see cref="ProjectWeeklyPlan"/> et des saisies de temps liées
/// (<c>TimeEntry.ProjectId</c>), pour respecter l'absence de recalcul rétroactif silencieux
/// (CLAUDE.md §7). Budget ajusté, rallonges et <c>Budget</c>/<c>BudgetVersion</c> sont
/// explicitement différés au Lot 5 : seul un budget initial simple est porté ici.
/// </summary>
public class Project : AuditableEntity
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public Guid ApplicationId { get; set; }
    public ApplicationReference Application { get; set; } = null!;

    public string? DescriptionCourte { get; set; }

    public Guid PiloteId { get; set; }
    public Resource Pilote { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid StatusId { get; set; }
    public ProjectStatus Status { get; set; } = null!;

    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinPrevueInitiale { get; set; }
    public DateOnly? DateFinAjustee { get; set; }
    public DateOnly? DateFinReelle { get; set; }

    public decimal? BudgetInitial { get; set; }
    public ProjectRiskLevel NiveauRisque { get; set; } = ProjectRiskLevel.Faible;
    public string? Commentaire { get; set; }

    public ICollection<ProjectParticipant> Participants { get; set; } = new List<ProjectParticipant>();
    public ICollection<ProjectPlanVersion> PlanVersions { get; set; } = new List<ProjectPlanVersion>();
}
