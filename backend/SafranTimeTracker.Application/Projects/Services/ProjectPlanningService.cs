using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Capacity.Services;
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
    IReadRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    AvailabilityService availabilityService,
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

    /// <summary>Lecture des lignes hebdomadaires déjà enregistrées pour une version (écart constaté
    /// à l'ouverture du Lot 10 : SetWeeklyPlansAsync n'était qu'en écriture, sans moyen de relire
    /// la grille pour l'affichage/la pré-remplir avant modification).</summary>
    public async Task<IReadOnlyList<ProjectWeeklyPlanDto>> GetWeeklyPlansAsync(Guid versionId, CancellationToken cancellationToken = default) =>
        await weeklyPlanRepository.Query()
            .Where(w => w.ProjectPlanVersionId == versionId)
            .OrderBy(w => w.WeekStartDate).ThenBy(w => w.ResourceId)
            .ProjectToType<ProjectWeeklyPlanDto>()
            .ToListAsync(cancellationToken);

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
                var coutReelConsomme = await GetCoutReelConsommeAsync(projectId, cancellationToken);
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

    /// <summary>
    /// Cahier des charges §18.2 (vue transverse tous projets, "tableau semaine par semaine") : une
    /// ligne par (Projet, Ressource, Semaine), agrégée côté serveur — décision actée avec
    /// l'utilisateur à l'ouverture du Lot 10 pour remplacer une agrégation par N appels frontend
    /// (GET /projects/{id}/planning répété par projet), coûteuse à maintenir et incompatible avec
    /// une pagination/un tri serveur corrects. Réutilise ProjectPlanningCalculator (aucune formule
    /// dupliquée) et AvailabilityService.GetAvailabilityAsync (§29.1-29.3, un seul appel par couple
    /// (ressource, semaine) distinct, jamais par ligne). "Sous-charge" (§29.6) n'est volontairement
    /// pas filtrable : elle exige un seuil configurable absent de Settings (même écart que
    /// AvailabilityResultDto, Lot 3/9, docs/BACKLOG_METIER.md) — inventer un seuil non validé est
    /// exclu (CLAUDE.md §7). Aucune donnée financière : pas de garde FINANCIAL_DATA_VIEW.
    /// </summary>
    public async Task<PagedResult<ProjectPlanningRowDto>> GetOverviewAsync(
        PaginationQuery pagination, Guid? projectId, Guid? resourceId, Guid? serviceId,
        Guid? departmentId, Guid? teamId, DateOnly? from, DateOnly? to, bool? surcharge,
        CancellationToken cancellationToken = default)
    {
        var query = weeklyPlanRepository.Query()
            .Where(w => w.ProjectPlanVersion.Statut == ProjectPlanVersionStatus.Active);

        if (projectId is not null) query = query.Where(w => w.ProjectPlanVersion.ProjectId == projectId);
        if (resourceId is not null) query = query.Where(w => w.ResourceId == resourceId);
        if (serviceId is not null) query = query.Where(w => w.Resource.ServiceId == serviceId);
        if (departmentId is not null) query = query.Where(w => w.Resource.DepartmentId == departmentId);
        if (teamId is not null) query = query.Where(w => w.Resource.TeamId == teamId);
        if (from is not null) query = query.Where(w => w.WeekStartDate >= from);
        if (to is not null) query = query.Where(w => w.WeekStartDate <= to);

        // "Surcharge" (§29.6) est un attribut calculé (capacité réelle via AvailabilityService) non
        // traduisible en prédicat SQL sans dupliquer le calcul (CLAUDE.md §3) : seul ce filtre exige
        // encore une matérialisation complète des lignes candidates avant pagination (même principe
        // que ProjectService.GetListAsync, branche alerteBudget). Sans ce filtre (cas majoritaire),
        // les clés (Projet, Ressource, Semaine) sont triées/paginées en base ; seule la page retournée
        // déclenche le calcul de charge réalisée/capacité (sous-lot 14.4 de l'audit du Lot 14).
        if (surcharge is not null)
        {
            return await GetOverviewFilteredBySurchargeAsync(query, pagination, surcharge.Value, cancellationToken);
        }

        var keysQuery = query
            .Select(w => new { ProjectId = w.ProjectPlanVersion.ProjectId, w.ResourceId, w.WeekStartDate })
            .Distinct();

        var totalCount = await keysQuery.CountAsync(cancellationToken);
        var pageKeys = await keysQuery
            .OrderBy(k => k.ProjectId).ThenBy(k => k.ResourceId).ThenBy(k => k.WeekStartDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(k => new WeeklyPlanKey(k.ProjectId, k.ResourceId, k.WeekStartDate))
            .ToListAsync(cancellationToken);

        var items = await BuildRowsAsync(query, pageKeys, cancellationToken);
        return new PagedResult<ProjectPlanningRowDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    private async Task<PagedResult<ProjectPlanningRowDto>> GetOverviewFilteredBySurchargeAsync(
        IQueryable<ProjectWeeklyPlan> query, PaginationQuery pagination, bool surcharge, CancellationToken cancellationToken)
    {
        var allKeys = await query
            .Select(w => new { ProjectId = w.ProjectPlanVersion.ProjectId, w.ResourceId, w.WeekStartDate })
            .Distinct()
            .OrderBy(k => k.ProjectId).ThenBy(k => k.ResourceId).ThenBy(k => k.WeekStartDate)
            .Select(k => new WeeklyPlanKey(k.ProjectId, k.ResourceId, k.WeekStartDate))
            .ToListAsync(cancellationToken);

        var rows = await BuildRowsAsync(query, allKeys, cancellationToken);
        var filtered = rows.Where(r => r.Surcharge == surcharge).ToList();

        var totalCount = filtered.Count;
        var page = filtered.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();

        return new PagedResult<ProjectPlanningRowDto> { Items = page, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    private readonly record struct WeeklyPlanKey(Guid ProjectId, Guid ResourceId, DateOnly WeekStartDate);

    /// <summary>Calcule les lignes (charge réalisée, capacité, surcharge) pour un ensemble de clés
    /// (Projet, Ressource, Semaine) déjà déterminé — la charge initiale/ajustée n'est relue qu'au
    /// périmètre de ces clés (projets/ressources/semaines concernés), jamais la table entière.</summary>
    private async Task<List<ProjectPlanningRowDto>> BuildRowsAsync(
        IQueryable<ProjectWeeklyPlan> query, List<WeeklyPlanKey> keys, CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return [];
        }

        var projectIds = keys.Select(k => k.ProjectId).Distinct().ToList();
        var resourceIds = keys.Select(k => k.ResourceId).Distinct().ToList();
        var weekStarts = keys.Select(k => k.WeekStartDate).Distinct().ToList();

        var rawLines = await query
            .Where(w => projectIds.Contains(w.ProjectPlanVersion.ProjectId)
                && resourceIds.Contains(w.ResourceId) && weekStarts.Contains(w.WeekStartDate))
            .Select(w => new
            {
                ProjectId = w.ProjectPlanVersion.ProjectId,
                w.ResourceId,
                w.WeekStartDate,
                w.ProjectPlanVersion.Type,
                w.ChargePlanifieeHeures
            })
            .ToListAsync(cancellationToken);

        var keySet = keys.ToHashSet();

        // Initiale et Ajustée (Active) peuvent coexister à la même clé (Projet, Ressource, Semaine) :
        // regroupées ici, jamais une troisième version "réalisé" saisie manuellement (§18.3).
        var groupedByKey = rawLines
            .Where(l => keySet.Contains(new WeeklyPlanKey(l.ProjectId, l.ResourceId, l.WeekStartDate)))
            .GroupBy(l => new WeeklyPlanKey(l.ProjectId, l.ResourceId, l.WeekStartDate))
            .ToDictionary(
                g => g.Key,
                g => (
                    ChargeInitiale: g.Where(l => l.Type == ProjectPlanVersionType.Initial).Sum(l => l.ChargePlanifieeHeures),
                    ChargeAjustee: g.Any(l => l.Type == ProjectPlanVersionType.Ajuste)
                        ? g.Where(l => l.Type == ProjectPlanVersionType.Ajuste).Sum(l => l.ChargePlanifieeHeures)
                        : (decimal?)null));

        var minWeek = weekStarts.Min();
        var maxWeekEnd = weekStarts.Max().AddDays(6);

        // Réalisé (§18.1) provient exclusivement des saisies de temps, jamais saisi manuellement.
        var timeEntries = await timeEntryRepository.Query()
            .Where(t => t.Statut == ReferentialStatus.Actif && t.ProjectId != null
                && projectIds.Contains(t.ProjectId!.Value) && resourceIds.Contains(t.ResourceId)
                && t.Date >= minWeek && t.Date <= maxWeekEnd)
            .Select(t => new { ProjectId = t.ProjectId!.Value, t.ResourceId, t.Date, t.DureeHeures })
            .ToListAsync(cancellationToken);

        var capacityCache = new Dictionary<(Guid ResourceId, DateOnly WeekStart), decimal>();

        var rows = new List<ProjectPlanningRowDto>(keys.Count);
        foreach (var key in keys)
        {
            var (chargeInitiale, chargeAjustee) = groupedByKey[key];
            var chargeRealisee = timeEntries
                .Where(t => t.ProjectId == key.ProjectId && t.ResourceId == key.ResourceId
                    && t.Date >= key.WeekStartDate && t.Date <= key.WeekStartDate.AddDays(6))
                .Sum(t => t.DureeHeures);
            var chargePrevue = chargeAjustee ?? chargeInitiale;

            var capacityKey = (key.ResourceId, key.WeekStartDate);
            if (!capacityCache.TryGetValue(capacityKey, out var capaciteReelle))
            {
                var availability = await availabilityService.GetAvailabilityAsync(
                    key.ResourceId, key.WeekStartDate, key.WeekStartDate.AddDays(6), cancellationToken);
                capaciteReelle = availability?.CapaciteReelle ?? 0m;
                capacityCache[capacityKey] = capaciteReelle;
            }

            rows.Add(new ProjectPlanningRowDto
            {
                ProjectId = key.ProjectId,
                ResourceId = key.ResourceId,
                WeekStartDate = key.WeekStartDate,
                ChargePlanifieeInitiale = chargeInitiale,
                ChargePlanifieeAjustee = chargeAjustee,
                ChargeRealisee = chargeRealisee,
                EcartPrevuRealise = chargeRealisee - chargePrevue,
                CapaciteReelle = capaciteReelle,
                Surcharge = chargePrevue > capaciteReelle
            });
        }

        return rows;
    }

    /// <summary>Anciennement porté par ProjectService (Lot 4) — déplacé ici (Lot 10) pour que
    /// ProjectService puisse à son tour dépendre de ProjectPlanningService (filtre "alerte budget"
    /// de GET /projects, §16.1) sans dépendance circulaire entre les deux services.</summary>
    private async Task<decimal> GetCoutReelConsommeAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await snapshotRepository.Query()
            .Where(s => s.TimeEntry.ProjectId == projectId)
            .SumAsync(s => s.CoutReelCalcule ?? 0, cancellationToken);
}
