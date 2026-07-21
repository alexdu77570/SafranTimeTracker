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

/// <summary>Ligne de la vue transverse "Planning projet" (§18.2, GET /api/v1/project-planning) :
/// une par (Projet, Ressource, Semaine), agrégée côté serveur (ProjectPlanningService.GetOverviewAsync).</summary>
public class ProjectPlanningRowDto
{
    public Guid ProjectId { get; set; }
    public Guid ResourceId { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public decimal ChargePlanifieeInitiale { get; set; }

    /// <summary>Null si aucune version Ajustée Active n'existe pour ce projet à cette semaine.</summary>
    public decimal? ChargePlanifieeAjustee { get; set; }

    /// <summary>Provient exclusivement des saisies de temps (§18.3), jamais saisi manuellement.</summary>
    public decimal ChargeRealisee { get; set; }

    public decimal EcartPrevuRealise { get; set; }
    public decimal CapaciteReelle { get; set; }

    /// <summary>§29.6 : charge planifiée (ajustée, sinon initiale) supérieure à la capacité réelle.</summary>
    public bool Surcharge { get; set; }
}
