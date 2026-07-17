using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users.Services;

/// <summary>Service en lecture seule (référentiel des permissions, seedé — pas de Create/Update/Delete ici).</summary>
public class PermissionService(IRepository<Permission> repository)
{
    public async Task<PagedResult<PermissionDto>> GetListAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<PermissionDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<PermissionDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(p => p.Id == id).ProjectToType<PermissionDto>().FirstOrDefaultAsync(cancellationToken);
}
