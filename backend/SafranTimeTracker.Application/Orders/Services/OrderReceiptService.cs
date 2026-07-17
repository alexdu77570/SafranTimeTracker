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
/// Réceptions partielles d'une commande (règle métier validée Lot 6 : le vocabulaire "Demande
/// d'achat → Commande → Réceptions partielles → Clôture" se représente par la machine d'état
/// <see cref="Order"/> existante — inchangée depuis le Lot 5 — complétée par ces événements
/// répétables). Append-only : aucune mise à jour, une correction est une nouvelle réception
/// (éventuellement négative, explicitement tracée). Le total réceptionné n'est jamais stocké en
/// double : toujours recalculé par somme des <see cref="OrderReceipt"/> (voir
/// <see cref="GetSummaryAsync"/>). La clôture d'une commande (<c>OrderService.CloseAsync</c>)
/// n'est volontairement pas conditionnée au solde réceptionné : les règles de transition validées
/// et testées au Lot 5 sont conservées telles quelles, pour ne pas régresser un lot déjà tagué —
/// un futur renforcement de cette règle est un choix produit à part entière, pas une omission.
/// </summary>
public class OrderReceiptService(
    IRepository<OrderReceipt> repository,
    IRepository<Order> orderRepository,
    IReadRepository<OrderStatus> orderStatusRepository,
    AuditService auditService,
    ICurrentUser currentUser)
{
    private const string StatusCloturee = "CLOTUREE";

    public async Task<PagedResult<OrderReceiptDto>> GetListAsync(
        Guid orderId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query().Where(r => r.OrderId == orderId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<OrderReceiptDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderReceiptDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<OrderReceiptSummaryDto?> GetSummaryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var totalAmount = await GetTotalReceivedAmountAsync(orderId, cancellationToken);
        var totalDays = await GetTotalReceivedDaysAsync(orderId, cancellationToken);

        return new OrderReceiptSummaryDto
        {
            TotalReceivedAmount = totalAmount,
            TotalReceivedDays = totalDays,
            RemainingReceivableAmount = order.BudgetFinancierAjuste - totalAmount,
            RemainingReceivableDays = order.BudgetJoursAjuste.HasValue ? order.BudgetJoursAjuste.Value - totalDays : null
        };
    }

    public async Task<OrderReceiptDto?> CreateAsync(Guid orderId, OrderReceiptCreateRequest request, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var status = await orderStatusRepository.GetByIdAsync(order.StatusId, cancellationToken);
        if (status?.Code == StatusCloturee)
        {
            throw new BusinessConflictException("Impossible d'ajouter une réception à une commande Clôturée : la réouvrir au préalable.");
        }

        if (request.ReceivedAmount is null && request.ReceivedDays is null)
        {
            throw new BusinessConflictException("Une réception doit indiquer un montant ou un nombre de jours reçus.");
        }
        if (request.ReceivedAmount is not null && request.ReceivedDays is not null)
        {
            throw new BusinessConflictException("Une réception ne peut porter à la fois sur un montant et sur des jours.");
        }

        if (request.ReceivedAmount is > 0)
        {
            var alreadyReceived = await GetTotalReceivedAmountAsync(orderId, cancellationToken);
            var remaining = order.BudgetFinancierAjuste - alreadyReceived;
            if (request.ReceivedAmount.Value > remaining && !currentUser.HasPermission(PermissionCodes.OrderReceiptOverride))
            {
                throw new BusinessConflictException(
                    $"Le montant réceptionné ({request.ReceivedAmount.Value}) dépasse le reste réceptionnable ({remaining}), "
                    + "sauf permission dédiée (ORDER_RECEIPT_OVERRIDE).");
            }
        }
        else if (request.ReceivedDays is > 0)
        {
            var alreadyReceivedDays = await GetTotalReceivedDaysAsync(orderId, cancellationToken);
            var remainingDays = (order.BudgetJoursAjuste ?? 0) - alreadyReceivedDays;
            if (request.ReceivedDays.Value > remainingDays && !currentUser.HasPermission(PermissionCodes.OrderReceiptOverride))
            {
                throw new BusinessConflictException(
                    $"Le nombre de jours réceptionnés ({request.ReceivedDays.Value}) dépasse le reste réceptionnable ({remainingDays}), "
                    + "sauf permission dédiée (ORDER_RECEIPT_OVERRIDE).");
            }
        }

        var entity = new OrderReceipt
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ReceiptDate = request.ReceiptDate,
            ReceivedAmount = request.ReceivedAmount,
            ReceivedDays = request.ReceivedDays,
            Reason = request.Reason,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.Identifier
        };

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Receipt, nameof(Order), orderId, null, entity.Adapt<OrderReceiptDto>(), request.Reason, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<OrderReceiptDto>();
    }

    private async Task<decimal> GetTotalReceivedAmountAsync(Guid orderId, CancellationToken cancellationToken) =>
        await repository.Query().Where(r => r.OrderId == orderId).SumAsync(r => r.ReceivedAmount ?? 0, cancellationToken);

    private async Task<decimal> GetTotalReceivedDaysAsync(Guid orderId, CancellationToken cancellationToken) =>
        await repository.Query().Where(r => r.OrderId == orderId).SumAsync(r => r.ReceivedDays ?? 0, cancellationToken);
}
