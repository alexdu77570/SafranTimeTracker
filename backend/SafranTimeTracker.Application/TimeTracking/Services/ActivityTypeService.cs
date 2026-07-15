using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.TimeTracking.Services;

public class ActivityTypeService(IRepository<ActivityType> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<ActivityTypeDto>> GetListAsync(
        PaginationQuery pagination, bool? isRun, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (isRun is not null)
        {
            query = query.Where(a => a.IsRun == isRun);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Libelle)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ActivityTypeDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ActivityTypeDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ActivityTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(a => a.Id == id).ProjectToType<ActivityTypeDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ActivityTypeDto> CreateAsync(ActivityTypeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<ActivityType>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ActivityTypeDto>();
    }
}
