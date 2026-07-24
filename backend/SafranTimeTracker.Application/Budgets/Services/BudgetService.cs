using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Application.Budgets.Services;

/// <summary>
/// Cahier des charges §14. Ressource intégralement financière : pas de projection de champ (voir
/// BudgetDto), le contrôleur associé est entièrement gardé par FINANCIAL_DATA_VIEW
/// (RequirePermissionAttribute, même principe que le Lot 2). Consommé/reste/atterrissage/risque ne
/// sont jamais des colonnes : toujours calculés à la demande ici, jamais dans le contrôleur
/// (demande explicite de l'utilisateur, Lot 5). Le risque de dépassement réutilise
/// ProjectPlanningCalculator.CalculateBudgetRisk (§29.5/§14.3 : même formule "atterrissage financier
/// > budget ajusté"), pour ne pas dupliquer la règle entre Project et Budget.
/// </summary>
public class BudgetService(
    IRepository<Budget> repository, IRepository<BudgetVersion> versionRepository, IReadRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<BudgetDto>> GetListAsync(
        PaginationQuery pagination, Guid? projectId, Guid? orderId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (projectId is not null)
        {
            query = query.Where(b => b.ProjectId == projectId);
        }
        if (orderId is not null)
        {
            query = query.Where(b => b.OrderId == orderId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(b => b.Name)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<BudgetDto>(entities.Count);
        foreach (var entity in entities)
        {
            items.Add(await ToDtoAsync(entity, cancellationToken));
        }

        return new PagedResult<BudgetDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<BudgetDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : await ToDtoAsync(entity, cancellationToken);
    }

    public async Task<BudgetDto> CreateAsync(BudgetCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Budget>();
        entity.Id = Guid.NewGuid();
        entity.AdjustedAmount = request.InitialAmount;
        entity.Status = BudgetStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<BudgetDto?> UpdateAsync(Guid id, BudgetUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;
        entity.ConcurrencyStamp = Guid.NewGuid();

        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>§14.1 : pas de suppression physique (CLAUDE.md §7), la clôture en tient lieu.</summary>
    public Task<BudgetDto?> CloseAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, BudgetStatus.Cloture, "Le budget est déjà clôturé.", cancellationToken);

    public Task<BudgetDto?> ReactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, BudgetStatus.Actif, "Le budget est déjà actif.", cancellationToken);

    public async Task<PagedResult<BudgetVersionDto>> GetVersionsAsync(
        Guid budgetId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = versionRepository.Query().Where(v => v.BudgetId == budgetId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<BudgetVersionDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<BudgetVersionDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    /// <summary>§14.2 : chaque ajustement conserve ancienne valeur, nouvelle valeur, motif, auteur,
    /// date — jamais un simple remplacement silencieux du montant ajusté.</summary>
    public async Task<BudgetVersionDto?> AdjustAsync(Guid budgetId, BudgetAdjustRequest request, CancellationToken cancellationToken = default)
    {
        var budget = await repository.GetByIdAsync(budgetId, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        if (budget.Status == BudgetStatus.Cloture)
        {
            throw new BusinessConflictException("Impossible d'ajuster un budget clôturé : le réactiver au préalable.");
        }

        var now = DateTime.UtcNow;
        var version = new BudgetVersion
        {
            Id = Guid.NewGuid(),
            BudgetId = budgetId,
            OldValue = budget.AdjustedAmount,
            NewValue = request.NewValue,
            Reason = request.Reason,
            ReferencePiece = request.ReferencePiece,
            CreatedAt = now,
            CreatedBy = currentUser.Identifier
        };

        budget.AdjustedAmount = request.NewValue;
        budget.UpdatedAt = now;
        budget.UpdatedBy = currentUser.Identifier;
        budget.ConcurrencyStamp = Guid.NewGuid();

        await versionRepository.AddAsync(version, cancellationToken);
        await versionRepository.SaveChangesAsync(cancellationToken);

        return version.Adapt<BudgetVersionDto>();
    }

    private async Task<BudgetDto?> TransitionStatusAsync(Guid id, BudgetStatus targetStatus, string conflictMessage, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (entity.Status == targetStatus)
        {
            throw new BusinessConflictException(conflictMessage);
        }

        entity.Status = targetStatus;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;
        entity.ConcurrencyStamp = Guid.NewGuid();

        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<BudgetDto> ToDtoAsync(Budget entity, CancellationToken cancellationToken)
    {
        var dto = entity.Adapt<BudgetDto>();

        // La commande est le rattachement le plus fin quand les deux sont renseignés
        // (simplification documentée, docs/IMPLEMENTATION_STATUS.md) : évite un double comptage si
        // un projet et une de ses commandes portent chacun des saisies distinctes.
        var aggregate = entity.OrderId is not null
            ? await snapshotRepository.Query().Where(s => s.TimeEntry.OrderId == entity.OrderId).GroupBy(_ => 1)
                .Select(g => new { CoutReel = g.Sum(s => s.CoutReelCalcule ?? 0), CoutContrat = g.Sum(s => s.CoutContratCalcule ?? 0), Differentiel = g.Sum(s => s.DifferentielCalcule ?? 0) })
                .FirstOrDefaultAsync(cancellationToken)
            : await snapshotRepository.Query().Where(s => s.TimeEntry.ProjectId == entity.ProjectId).GroupBy(_ => 1)
                .Select(g => new { CoutReel = g.Sum(s => s.CoutReelCalcule ?? 0), CoutContrat = g.Sum(s => s.CoutContratCalcule ?? 0), Differentiel = g.Sum(s => s.DifferentielCalcule ?? 0) })
                .FirstOrDefaultAsync(cancellationToken);

        var coutReel = aggregate?.CoutReel ?? 0m;

        dto.CoutReelConsomme = coutReel;
        dto.CoutContractuelConsomme = aggregate?.CoutContrat ?? 0m;
        dto.Differentiel = aggregate?.Differentiel ?? 0m;
        dto.MontantRestant = entity.AdjustedAmount - coutReel;
        // Simplification MVP validée par l'utilisateur : pas de plan de charge associé à un budget
        // générique (contrairement au projet, §29.5), donc pas d'extrapolation possible.
        dto.AtterrissageEstime = coutReel;
        dto.RisqueDepassement = ProjectPlanningCalculator.CalculateBudgetRisk(coutReel, entity.AdjustedAmount);

        return dto;
    }
}
