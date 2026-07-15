using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Capacity.Services;

/// <summary>Variations de capacité d'une ressource (cahier des charges §10.5). Mêmes règles
/// d'intégrité que les historiques financiers (docs/DATABASE.md §5) : chevauchement -> 409.</summary>
public class ResourceCapacityPeriodService(IRepository<ResourceCapacityPeriod> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<ResourceCapacityPeriodDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (resourceId is not null)
        {
            query = query.Where(p => p.ResourceId == resourceId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.StartDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ResourceCapacityPeriodDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ResourceCapacityPeriodDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ResourceCapacityPeriodDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(p => p.Id == id).ProjectToType<ResourceCapacityPeriodDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ResourceCapacityPeriodDto> CreateAsync(ResourceCapacityPeriodCreateRequest request, CancellationToken cancellationToken = default)
    {
        var existingPeriods = await repository.Query()
            .Where(p => p.ResourceId == request.ResourceId && p.Status == ReferentialStatus.Actif)
            .Select(p => new { p.StartDate, p.EndDate })
            .ToListAsync(cancellationToken);

        if (existingPeriods.Any(p => DateRangeOverlap.Overlaps(p.StartDate, p.EndDate, request.StartDate, request.EndDate)))
        {
            throw new BusinessConflictException(
                "Une période de capacité active existe déjà pour cette ressource sur cette plage de dates (cahier des charges §10.5).");
        }

        var entity = request.Adapt<ResourceCapacityPeriod>();
        entity.Id = Guid.NewGuid();
        entity.Status = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceCapacityPeriodDto>();
    }
}
