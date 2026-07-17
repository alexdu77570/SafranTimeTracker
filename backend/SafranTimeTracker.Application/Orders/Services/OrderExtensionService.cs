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

namespace SafranTimeTracker.Application.Orders.Services;

/// <summary>
/// Rallonges de commande (cahier des charges §13.3). Toute rallonge augmente le budget ajusté,
/// conserve le budget initial, reste visible dans l'historique (append-only, jamais corrigée) et
/// met à jour les prévisions (budget/jours/date ajustés de la commande, dans la même transaction).
/// Auditée (§28.3 "rallonge").
/// </summary>
public class OrderExtensionService(
    IRepository<OrderExtension> repository, IRepository<Order> orderRepository, IReadRepository<OrderStatus> orderStatusRepository,
    AuditService auditService, ICurrentUser currentUser)
{
    public async Task<PagedResult<OrderExtensionDto>> GetListAsync(
        Guid orderId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query().Where(e => e.OrderId == orderId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<OrderExtensionDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderExtensionDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<OrderExtensionDto?> CreateAsync(Guid orderId, OrderExtensionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var status = await orderStatusRepository.GetByIdAsync(order.StatusId, cancellationToken);
        if (status?.Code == "CLOTUREE")
        {
            throw new BusinessConflictException("Impossible d'ajouter une rallonge à une commande Clôturée : la réouvrir au préalable.");
        }

        var previousEndDate = order.DateFinAjustee ?? order.DateFinInitiale;
        if (request.NewEndDate < previousEndDate)
        {
            throw new BusinessConflictException("La nouvelle date de fin d'une rallonge ne peut pas être antérieure à la date de fin actuelle.");
        }

        var now = DateTime.UtcNow;
        var entity = new OrderExtension
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ExtensionDate = DateOnly.FromDateTime(now),
            AmountAdded = request.AmountAdded,
            DaysAdded = request.DaysAdded,
            PreviousEndDate = previousEndDate,
            NewEndDate = request.NewEndDate,
            Reason = request.Reason,
            Comment = request.Comment,
            CreatedAt = now,
            CreatedBy = currentUser.Identifier
        };

        order.BudgetFinancierAjuste += request.AmountAdded;
        if (request.DaysAdded is not null)
        {
            order.BudgetJoursAjuste = (order.BudgetJoursAjuste ?? 0) + request.DaysAdded.Value;
        }
        order.DateFinAjustee = request.NewEndDate;
        order.UpdatedAt = now;
        order.UpdatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Extension, nameof(Order), orderId, null, entity.Adapt<OrderExtensionDto>(), request.Reason, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<OrderExtensionDto>();
    }
}
