using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>
/// Cahier des charges §16. Le sous-objet financier (budget, coûts consommés, différentiel) est
/// omis sans FINANCIAL_DATA_VIEW (CLAUDE.md §13, §17 : "budgets"/"montants" sont des données
/// financières). Coûts consommés agrégés depuis TimeEntryFinancialSnapshot via TimeEntry.ProjectId
/// (§20.6), jamais recalculés aux taux actuels. Création, modification et archivage audités
/// (§28.3, Lot 6) — les entrées d'audit ne portent que les champs non financiers du projet, jamais
/// FinancialSummary (calculé, pas persisté sur l'entité).
/// </summary>
public class ProjectService(
    IRepository<Project> repository,
    IReadRepository<ProjectStatus> statusRepository,
    IReadRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    ProjectPlanningService planningService,
    AuditService auditService,
    ICurrentUser currentUser)
{
    private const string StatusActif = "ACTIF";
    private const string StatusArchive = "ARCHIVE";

    public async Task<PagedResult<ProjectDto>> GetListAsync(
        PaginationQuery pagination, Guid? statusId, Guid? applicationId, Guid? piloteId,
        Guid? departmentId, Guid? serviceId, Guid? teamId, ProjectRiskLevel? niveauRisque,
        DateOnly? from, DateOnly? to, bool? alertePlanning, bool? alerteBudget,
        CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (statusId is not null) query = query.Where(p => p.StatusId == statusId);
        if (applicationId is not null) query = query.Where(p => p.ApplicationId == applicationId);
        if (piloteId is not null) query = query.Where(p => p.PiloteId == piloteId);
        if (departmentId is not null) query = query.Where(p => p.DepartmentId == departmentId);
        if (serviceId is not null) query = query.Where(p => p.ServiceId == serviceId);
        if (teamId is not null) query = query.Where(p => p.TeamId == teamId);
        if (niveauRisque is not null) query = query.Where(p => p.NiveauRisque == niveauRisque);
        // "Période" (§16.1) : projets dont l'intervalle [DateDebut, date de fin de référence] intersecte [from, to].
        if (from is not null) query = query.Where(p => (p.DateFinAjustee ?? p.DateFinPrevueInitiale) >= from);
        if (to is not null) query = query.Where(p => p.DateDebut <= to);
        if (alertePlanning is not null)
        {
            // Risque planning (§29.5) = date ajustée postérieure à la date initiale : traduisible
            // directement en SQL, contrairement à "alerte budget" ci-dessous (ProjectPlanningCalculator
            // réutilisé tel quel, même prédicat que ProjectPlanningService.GetSynthesisAsync).
            query = alertePlanning.Value
                ? query.Where(p => p.DateFinAjustee != null && p.DateFinAjustee > p.DateFinPrevueInitiale)
                : query.Where(p => p.DateFinAjustee == null || p.DateFinAjustee <= p.DateFinPrevueInitiale);
        }

        var hasFinancialAccess = currentUser.HasPermission(PermissionCodes.FinancialDataView);

        if (alerteBudget is null)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            var entities = await query
                .OrderBy(p => p.Nom)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            var items = new List<ProjectDto>(entities.Count);
            foreach (var entity in entities)
            {
                items.Add(await ToDtoAsync(entity, hasFinancialAccess, cancellationToken));
            }

            return new PagedResult<ProjectDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
        }

        // "Alerte budget" (§16.1) dépend de l'atterrissage financier (ProjectPlanningCalculator, §29.5,
        // réutilisé sans duplication via ProjectPlanningService.GetSynthesisAsync) : non traduisible en
        // prédicat SQL (agrégation planning + snapshots financiers), donc matérialisation du candidat
        // filtré puis pagination en mémoire, sur le total réellement filtré (pas seulement la page
        // courante) — jeu de projets de taille modeste (données de démonstration).
        if (!hasFinancialAccess)
        {
            // RisqueBudget est toujours absent sans FINANCIAL_DATA_VIEW (même principe que
            // FinancialSummary, CLAUDE.md §13) : le filtre ne peut honnêtement retenir aucun projet
            // plutôt que d'ignorer silencieusement le filtre demandé.
            return new PagedResult<ProjectDto> { Items = [], Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = 0 };
        }

        var candidates = await query.OrderBy(p => p.Nom).ToListAsync(cancellationToken);
        var matched = new List<Project>(candidates.Count);
        foreach (var candidate in candidates)
        {
            var synthesis = await planningService.GetSynthesisAsync(candidate.Id, cancellationToken);
            if (synthesis?.RisqueBudget == alerteBudget.Value)
            {
                matched.Add(candidate);
            }
        }

        var page = matched.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToList();
        var pageItems = new List<ProjectDto>(page.Count);
        foreach (var entity in page)
        {
            pageItems.Add(await ToDtoAsync(entity, hasFinancialAccess, cancellationToken));
        }

        return new PagedResult<ProjectDto> { Items = pageItems, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = matched.Count };
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : await ToDtoAsync(entity, currentUser.HasPermission(PermissionCodes.FinancialDataView), cancellationToken);
    }

    public async Task<ProjectDto> CreateAsync(ProjectCreateRequest request, CancellationToken cancellationToken = default)
    {
        var activeStatusId = await statusRepository.Query()
            .Where(s => s.Code == StatusActif)
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        var entity = request.Adapt<Project>();
        entity.Id = Guid.NewGuid();
        entity.StatusId = activeStatusId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(Project), entity.Id, null, entity.Adapt<ProjectDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<ProjectDto?> UpdateAsync(Guid id, ProjectUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<ProjectDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(Project), id, oldValue, entity.Adapt<ProjectDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>§16.3 : suppression interdite (CLAUDE.md §7), l'archivage en tient lieu.</summary>
    public Task<ProjectDto?> ArchiveAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusArchive, "Le projet est déjà archivé.", cancellationToken);

    public Task<ProjectDto?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusActif, "Le projet est déjà actif.", cancellationToken);

    private async Task<ProjectDto?> TransitionStatusAsync(Guid id, string targetStatusCode, string conflictMessage, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var targetStatus = await statusRepository.Query().FirstAsync(s => s.Code == targetStatusCode, cancellationToken);
        if (entity.StatusId == targetStatus.Id)
        {
            throw new BusinessConflictException(conflictMessage);
        }

        var oldStatusId = entity.StatusId;
        entity.StatusId = targetStatus.Id;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        var action = targetStatusCode == StatusArchive ? AuditActions.Archive : AuditActions.Reactivate;
        await auditService.RecordAsync(
            action, nameof(Project), id, new { StatusId = oldStatusId }, new { entity.StatusId }, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<ProjectDto> ToDtoAsync(Project entity, bool hasFinancialAccess, CancellationToken cancellationToken)
    {
        var dto = entity.Adapt<ProjectDto>();
        dto.FinancialSummary = hasFinancialAccess
            ? await BuildFinancialSummaryAsync(entity.Id, entity.BudgetInitial, cancellationToken)
            : null;
        return dto;
    }

    private async Task<ProjectFinancialSummaryDto> BuildFinancialSummaryAsync(Guid projectId, decimal? budgetInitial, CancellationToken cancellationToken)
    {
        var aggregate = await snapshotRepository.Query()
            .Where(s => s.TimeEntry.ProjectId == projectId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                CoutReel = g.Sum(s => s.CoutReelCalcule ?? 0),
                CoutContrat = g.Sum(s => s.CoutContratCalcule ?? 0),
                Differentiel = g.Sum(s => s.DifferentielCalcule ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var coutReel = aggregate?.CoutReel ?? 0m;

        return new ProjectFinancialSummaryDto
        {
            BudgetInitial = budgetInitial,
            CoutReelConsomme = coutReel,
            CoutContractuelConsomme = aggregate?.CoutContrat ?? 0m,
            Differentiel = aggregate?.Differentiel ?? 0m,
            BudgetRestant = budgetInitial.HasValue ? budgetInitial - coutReel : null
        };
    }

}
