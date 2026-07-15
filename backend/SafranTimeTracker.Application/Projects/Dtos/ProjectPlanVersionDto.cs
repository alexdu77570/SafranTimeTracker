using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Projects.Dtos;

public class ProjectPlanVersionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectPlanVersionType Type { get; set; }
    public ProjectPlanVersionStatus Statut { get; set; }
    public string? Motif { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>Création de la version Initiale (§18.3) — rien à justifier à la création du projet,
/// contrairement à une version Ajustée (voir <see cref="ProjectPlanVersionAdjustmentRequest"/>).</summary>
public class ProjectPlanVersionCreateRequest
{
    public string? Motif { get; set; }
}

/// <summary>Motif obligatoire pour une version Ajustée (§18.3). Type DTO distinct de
/// <see cref="ProjectPlanVersionCreateRequest"/> (bien que même forme) pour que chaque endpoint
/// porte son propre validateur sans ambiguïté d'enregistrement DI.</summary>
public class ProjectPlanVersionAdjustmentRequest
{
    public string Motif { get; set; } = string.Empty;
}

public class ProjectWeeklyPlanDto
{
    public Guid Id { get; set; }
    public Guid ProjectPlanVersionId { get; set; }
    public Guid ResourceId { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public decimal ChargePlanifieeHeures { get; set; }
}

public class ProjectWeeklyPlanLineRequest
{
    public Guid ResourceId { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public decimal ChargePlanifieeHeures { get; set; }
}
