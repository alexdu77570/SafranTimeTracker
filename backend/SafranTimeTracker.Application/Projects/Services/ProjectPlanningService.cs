using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>
/// Versions de planning (§18.3) et seul point de calcul des écarts/risques (§29.5,
/// docs/ARCHITECTURE.md §2 — même principe que FinancialCalculationService pour §20). Précision
/// actée avec l'utilisateur (Lot 4) : une seule version Initiale par projet (immuable) ; plusieurs
/// versions Ajustées possibles, une seule Active à la fois — la précédente bascule automatiquement
/// à Archivee, jamais supprimée.
/// </summary>
public class ProjectPlanningService(
    IRepository<ProjectPlanVersion> versionRepository,
    IRepository<ProjectWeeklyPlan> weeklyPlanRepository,
    IReadRepository<Project> projectRepository,
    IReadRepository<TimeEntry> timeEntryRepository,
    IReadRepository<Budget> budgetRepository,
    ProjectService projectService,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<ProjectPlanVersionDto>> GetVersionsAsync(
        Guid projectId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = versionRepository.Query().Where(v => v.ProjectId == projectId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ProjectPlanVersionDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectPlanVersionDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<ProjectPlanVersionDto> CreateInitialVersionAsync(
        Guid projectId, ProjectPlanVersionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var hasInitial = await versionRepository.Query()
            .AnyAsync(v => v.ProjectId == projectId && v.Type == ProjectPlanVersionType.Initial, cancellationToken);
        if (hasInitial)
        {
            throw new BusinessConflictException("Une version Initiale existe déjà pour ce projet (cahier des charges §18.3).");
        }

        var entity = new ProjectPlanVersion
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Type = ProjectPlanVersionType.Initial,
            Statut = ProjectPlanVersionStatus.Active,
            Motif = request.Motif,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.Identifier
        };

        await versionRepository.AddAsync(entity, cancellationToken);
        await versionRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ProjectPlanVersionDto>();
    }

    /// <summary>Archive automatiquement la version Ajustée Active précédente, s'il en existe une
    /// (précision actée : une seule Active à la fois, l'historique n'est jamais supprimé).</summary>
    public async Task<ProjectPlanVersionDto> CreateAdjustedVersionAsync(
        Guid projectId, ProjectPlanVersionAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        var activeVersionId = await versionRepository.Query()
            .Where(v => v.ProjectId == projectId && v.Type == ProjectPlanVersionType.Ajuste && v.Statut == ProjectPlanVersionStatus.Active)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeVersionId is not null)
        {
            var previousActive = await versionRepository.GetByIdAsync(activeVersionId.Value, cancellationToken);
            previousActive!.Statut = ProjectPlanVersionStatus.Archivee;
            previousActive.UpdatedAt = DateTime.UtcNow;
            previousActive.UpdatedBy = currentUser.Identifier;
        }

        var entity = new ProjectPlanVersion
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Type = ProjectPlanVersionType.Ajuste,
            Statut = ProjectPlanVersionStatus.Active,
            Motif = request.Motif,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.Identifier
        };

        await versionRepository.AddAsync(entity, cancellationToken);
        await versionRepository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ProjectPlanVersionDto>();
    }

    public async Task<IReadOnlyList<ProjectWeeklyPlanDto>> SetWeeklyPlansAsync(
        Guid versionId, IReadOnlyList<ProjectWeeklyPlanLineRequest> lines, CancellationToken cancellationToken = default)
    {
        var results = new List<ProjectWeeklyPlanDto>(lines.Count);

        foreach (var line in lines)
        {
            var existingId = await weeklyPlanRepository.Query()
                .Where(w => w.ProjectPlanVersionId == versionId && w.ResourceId == line.ResourceId && w.WeekStartDate == line.WeekStartDate)
                .Select(w => (Guid?)w.Id)
                .FirstOrDefaultAsync(cancellationToken);

            ProjectWeeklyPlan entity;
            if (existingId is not null)
            {
                entity = (await weeklyPlanRepository.GetByIdAsync(existingId.Value, cancellationToken))!;
                entity.ChargePlanifieeHeures = line.ChargePlanifieeHeures;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = currentUser.Identifier;
            }
            else
            {
                entity = new ProjectWeeklyPlan
                {
                    Id = Guid.NewGuid(),
                    ProjectPlanVersionId = versionId,
                    ResourceId = line.ResourceId,
                    WeekStartDate = line.WeekStartDate,
                    ChargePlanifieeHeures = line.ChargePlanifieeHeures,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Identifier
                };
                await weeklyPlanRepository.AddAsync(entity, cancellationToken);
            }

            results.Add(entity.Adapt<ProjectWeeklyPlanDto>());
        }

        await weeklyPlanRepository.SaveChangesAsync(cancellationToken);
        return results;
    }

    /// <summary>Cahier des charges §18.1 (comparaison initial/ajusté/réalisé) et §29.5 (formules
    /// d'écart et de risque) — seul endroit où ces formules sont calculées.</summary>
    public async Task<ProjectPlanningSynthesisDto?> GetSynthesisAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var chargeInitiale = await weeklyPlanRepository.Query()
            .Where(w => w.ProjectPlanVersion.ProjectId == projectId && w.ProjectPlanVersion.Type == ProjectPlanVersionType.Initial)
            .SumAsync(w => (decimal?)w.ChargePlanifieeHeures, cancellationToken) ?? 0m;

        var hasActiveAdjustment = await versionRepository.Query()
            .AnyAsync(v => v.ProjectId == projectId && v.Type == ProjectPlanVersionType.Ajuste && v.Statut == ProjectPlanVersionStatus.Active, cancellationToken);

        decimal? chargeAjustee = null;
        if (hasActiveAdjustment)
        {
            chargeAjustee = await weeklyPlanRepository.Query()
                .Where(w => w.ProjectPlanVersion.ProjectId == projectId
                    && w.ProjectPlanVersion.Type == ProjectPlanVersionType.Ajuste
                    && w.ProjectPlanVersion.Statut == ProjectPlanVersionStatus.Active)
                .SumAsync(w => (decimal?)w.ChargePlanifieeHeures, cancellationToken) ?? 0m;
        }

        var chargeConsommee = await timeEntryRepository.Query()
            .Where(t => t.ProjectId == projectId && t.Statut == ReferentialStatus.Actif)
            .SumAsync(t => (decimal?)t.DureeHeures, cancellationToken) ?? 0m;

        var chargeMetrics = ProjectPlanningCalculator.CalculateChargeMetrics(chargeInitiale, chargeAjustee, chargeConsommee);
        var planningRisk = ProjectPlanningCalculator.CalculatePlanningRisk(project.DateFinPrevueInitiale, project.DateFinAjustee);

        decimal? atterrissageFinancier = null;
        bool? risqueBudget = null;
        if (currentUser.HasPermission(PermissionCodes.FinancialDataView))
        {
            // Le Budget lié au projet (Lot 5) porte le budget ajusté faisant foi (§14.2, après
            // rallonges/ajustements éventuels) ; à défaut, repli documenté sur Project.BudgetInitial
            // (écart Lot 4 : aucun Budget n'est nécessairement créé pour chaque projet).
            var budgetAjuste = await budgetRepository.Query()
                .Where(b => b.ProjectId == projectId)
                .Select(b => (decimal?)b.AdjustedAmount)
                .FirstOrDefaultAsync(cancellationToken) ?? project.BudgetInitial;

            if (budgetAjuste is not null)
            {
                var coutReelConsomme = await projectService.GetCoutReelConsommeAsync(projectId, cancellationToken);
                atterrissageFinancier = ProjectPlanningCalculator.CalculateAtterrissageFinancier(coutReelConsomme, chargeConsommee, chargeMetrics.AtterrissageCharge);
                risqueBudget = ProjectPlanningCalculator.CalculateBudgetRisk(atterrissageFinancier.Value, budgetAjuste.Value);
            }
        }

        return new ProjectPlanningSynthesisDto
        {
            ProjectId = projectId,
            ChargeInitiale = chargeInitiale,
            ChargeAjustee = chargeAjustee,
            ChargeConsommee = chargeConsommee,
            ChargeRestante = chargeMetrics.ChargeRestante,
            EcartCharge = chargeMetrics.EcartCharge,
            DeriveCharge = chargeMetrics.DeriveCharge,
            AtterrissageCharge = chargeMetrics.AtterrissageCharge,
            DerivePlanningJours = planningRisk.DerivePlanningJours,
            RisquePlanning = planningRisk.RisquePlanning,
            AtterrissageFinancier = atterrissageFinancier,
            RisqueBudget = risqueBudget
        };
    }
}
