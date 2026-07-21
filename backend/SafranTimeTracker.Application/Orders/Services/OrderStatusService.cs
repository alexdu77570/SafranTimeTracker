using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Application.Orders.Services;

/// <summary>
/// Référentiel de statuts de commande (cahier des charges §13.2, §30) : Brouillon, Active,
/// Suspendue, Consommée, Clôturée — lecture seule (écart constaté à l'ouverture du Lot 11, même
/// nature que ProjectStatus au Lot 10, corrigée ici plutôt que contournée par
/// `knownReferentials.ts`, décision validée par l'utilisateur). Aucune permission requise, même
/// principe d'ouverture que les autres référentiels non financiers.
/// </summary>
public class OrderStatusService(IReadRepository<OrderStatus> repository)
{
    public async Task<PagedResult<OrderStatusDto>> GetListAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Ordre)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<OrderStatusDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderStatusDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<OrderStatusDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(s => s.Id == id).ProjectToType<OrderStatusDto>().FirstOrDefaultAsync(cancellationToken);
}
