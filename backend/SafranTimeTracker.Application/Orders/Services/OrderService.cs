using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Time;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Orders.Services;

/// <summary>
/// Cahier des charges §13. Statuts pilotés par une vraie machine d'état (précision actée avec
/// l'utilisateur, Lot 5) : aucune transition libre, une commande Clôturée ne peut redevenir Active
/// que via <see cref="ReopenAsync"/> (motif obligatoire), jamais via <see cref="ActivateAsync"/>.
/// Le sous-objet financier est omis sans FINANCIAL_DATA_VIEW (CLAUDE.md §13), même principe que
/// <c>ProjectService</c>. Toute création/modification est auditée (§28.3).
///
/// Règle métier validée Lot 6 : le vocabulaire "Demande d'achat → Commande → Réceptions
/// partielles → Clôture" se représente techniquement par cette machine d'état (Brouillon ≈
/// Demande d'achat, Active ≈ Commande, Clôturée ≈ Clôture — inchangée depuis le Lot 5, aucune
/// régression) complétée par les événements <see cref="Orders.OrderReceipt"/> (Réceptions
/// partielles, <c>OrderReceiptService</c>) qui s'ajoutent par-dessus sans modifier cette machine
/// d'état. La société d'une commande (<see cref="Order.CompanyId"/>) est immuable après création :
/// <see cref="OrderUpdateRequest"/> ne porte volontairement pas ce champ. La ressource affectée
/// peut changer en cours de vie de la commande via <see cref="OrderUpdateRequest.AuthorizedResourceIds"/>.
/// </summary>
public class OrderService(
    IRepository<Order> repository,
    IReadRepository<OrderStatus> orderStatusRepository,
    IReadRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    IReadRepository<TimeEntry> timeEntryRepository,
    IReadRepository<SettingsEntity> settingsRepository,
    AuditService auditService,
    ICurrentUser currentUser)
{
    private const string StatusBrouillon = "BROUILLON";
    private const string StatusActive = "ACTIVE";
    private const string StatusSuspendue = "SUSPENDUE";
    private const string StatusConsommee = "CONSOMMEE";
    private const string StatusCloturee = "CLOTUREE";

    private static readonly string[] ActivatableFrom = [StatusBrouillon, StatusSuspendue, StatusConsommee];
    private static readonly string[] SuspendableFrom = [StatusActive];
    private static readonly string[] ConsumableFrom = [StatusActive];
    private static readonly string[] ClosableFrom = [StatusBrouillon, StatusActive, StatusSuspendue, StatusConsommee];

    public async Task<PagedResult<OrderDto>> GetListAsync(
        PaginationQuery pagination, Guid? companyId, Guid? statusId, Guid? projectId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (companyId is not null)
        {
            query = query.Where(o => o.CompanyId == companyId);
        }
        if (statusId is not null)
        {
            query = query.Where(o => o.StatusId == statusId);
        }
        if (projectId is not null)
        {
            query = query.Where(o => o.ProjectId == projectId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Include(o => o.AuthorizedResources)
            .OrderBy(o => o.Reference)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var hasFinancialAccess = currentUser.HasPermission(PermissionCodes.FinancialDataView);
        var items = new List<OrderDto>(entities.Count);
        foreach (var entity in entities)
        {
            items.Add(await ToDtoAsync(entity, hasFinancialAccess, cancellationToken));
        }

        return new PagedResult<OrderDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include requis : AuthorizedResources n'est jamais chargée par repository.GetByIdAsync
        // (FindAsync) ni par une simple Query() sans jointure explicite.
        var entity = await repository.Query().Include(o => o.AuthorizedResources).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return entity is null ? null : await ToDtoAsync(entity, currentUser.HasPermission(PermissionCodes.FinancialDataView), cancellationToken);
    }

    public async Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        var brouillonStatusId = await orderStatusRepository.Query()
            .Where(s => s.Code == StatusBrouillon)
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        var entity = request.Adapt<Order>();
        entity.Id = Guid.NewGuid();
        entity.StatusId = brouillonStatusId;
        // Le budget/jours/date "ajusté" démarre égal à l'engagement initial (cahier des charges
        // §13.2) : seule une rallonge (§13.3) le fait ensuite évoluer.
        entity.BudgetFinancierAjuste = request.BudgetFinancierInitial;
        entity.BudgetJoursAjuste = request.BudgetJoursInitial;
        entity.DateFinAjustee = request.DateFinInitiale;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;
        entity.AuthorizedResources = request.AuthorizedResourceIds
            .Select(resourceId => new OrderAuthorizedResource { OrderId = entity.Id, ResourceId = resourceId })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(AuditActions.Create, nameof(Order), entity.Id, null, entity.Adapt<OrderDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<OrderDto>();
        request.Adapt(entity);
        entity.AuthorizedResources = request.AuthorizedResourceIds
            .Select(resourceId => new OrderAuthorizedResource { OrderId = entity.Id, ResourceId = resourceId })
            .ToList();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(AuditActions.Update, nameof(Order), id, oldValue, entity.Adapt<OrderDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<OrderDto?> ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusActive, ActivatableFrom, cancellationToken);

    public Task<OrderDto?> SuspendAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusSuspendue, SuspendableFrom, cancellationToken);

    public Task<OrderDto?> MarkConsumedAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusConsommee, ConsumableFrom, cancellationToken);

    public Task<OrderDto?> CloseAsync(Guid id, CancellationToken cancellationToken = default) =>
        TransitionStatusAsync(id, StatusCloturee, ClosableFrom, cancellationToken);

    /// <summary>Seule action capable de sortir une commande de Clôturée (cahier des charges §13.4) :
    /// motif obligatoire, jamais une transition ordinaire.</summary>
    public async Task<OrderDto?> ReopenAsync(Guid id, OrderReopenRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var currentStatus = await orderStatusRepository.GetByIdAsync(entity.StatusId, cancellationToken);
        if (currentStatus?.Code != StatusCloturee)
        {
            throw new BusinessConflictException("Seule une commande Clôturée peut être réouverte.");
        }

        var activeStatusId = await orderStatusRepository.Query()
            .Where(s => s.Code == StatusActive)
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        entity.StatusId = activeStatusId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.StatusChange, nameof(Order), id,
            new { Status = StatusCloturee }, new { Status = StatusActive }, request.Motif, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<OrderDto?> TransitionStatusAsync(
        Guid id, string targetStatusCode, string[] allowedFromCodes, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var currentStatus = await orderStatusRepository.GetByIdAsync(entity.StatusId, cancellationToken);
        if (currentStatus is null || !allowedFromCodes.Contains(currentStatus.Code))
        {
            throw new BusinessConflictException(
                $"Transition impossible : la commande est au statut '{currentStatus?.Code}', "
                + $"seuls {string.Join(", ", allowedFromCodes)} permettent de passer à '{targetStatusCode}' (cahier des charges §13.2).");
        }

        var targetStatus = await orderStatusRepository.Query().FirstAsync(s => s.Code == targetStatusCode, cancellationToken);
        entity.StatusId = targetStatus.Id;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.StatusChange, nameof(Order), id,
            new { Status = currentStatus.Code }, new { Status = targetStatusCode }, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<OrderDto> ToDtoAsync(Order entity, bool hasFinancialAccess, CancellationToken cancellationToken)
    {
        var dto = entity.Adapt<OrderDto>();
        dto.FinancialSummary = hasFinancialAccess ? await BuildFinancialSummaryAsync(entity, cancellationToken) : null;
        return dto;
    }

    private async Task<OrderFinancialSummaryDto> BuildFinancialSummaryAsync(Order order, CancellationToken cancellationToken)
    {
        var aggregate = await snapshotRepository.Query()
            .Where(s => s.TimeEntry.OrderId == order.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                CoutReel = g.Sum(s => s.CoutReelCalcule ?? 0),
                CoutContrat = g.Sum(s => s.CoutContratCalcule ?? 0),
                Differentiel = g.Sum(s => s.DifferentielCalcule ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var heuresParJour = await settingsRepository.Query().Select(s => s.HeuresParJour).FirstAsync(cancellationToken);
        var heuresSaisies = await timeEntryRepository.Query()
            .Where(t => t.OrderId == order.Id)
            .SumAsync(t => (decimal?)t.DureeHeures, cancellationToken) ?? 0m;

        var coutReel = aggregate?.CoutReel ?? 0m;
        var consommationJours = heuresParJour > 0 ? heuresSaisies / heuresParJour : 0m;

        return new OrderFinancialSummaryDto
        {
            ConsommationJours = consommationJours,
            CoutReelConsomme = coutReel,
            CoutContractuelConsomme = aggregate?.CoutContrat ?? 0m,
            Differentiel = aggregate?.Differentiel ?? 0m,
            RestFinancier = order.BudgetFinancierAjuste - coutReel,
            RestJours = order.BudgetJoursAjuste.HasValue ? order.BudgetJoursAjuste.Value - consommationJours : null
        };
    }
}
