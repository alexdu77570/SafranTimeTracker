using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Application.Orders.Services;

public class OrderService(IRepository<Order> repository, IReadRepository<OrderStatus> orderStatusRepository)
{
    public async Task<PagedResult<OrderDto>> GetListAsync(
        PaginationQuery pagination, Guid? companyId, Guid? statusId, CancellationToken cancellationToken = default)
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

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(o => o.Reference)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<OrderDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(o => o.Id == id).ProjectToType<OrderDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<OrderDto> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        var brouillonStatusId = await orderStatusRepository.Query()
            .Where(s => s.Code == "BROUILLON")
            .Select(s => s.Id)
            .FirstAsync(cancellationToken);

        var entity = request.Adapt<Order>();
        entity.Id = Guid.NewGuid();
        entity.StatusId = brouillonStatusId;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;
        entity.AuthorizedResources = request.AuthorizedResourceIds
            .Select(resourceId => new OrderAuthorizedResource { OrderId = entity.Id, ResourceId = resourceId })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<OrderDto>();
    }
}
