using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Application.Milestones.Services;

public class MilestoneTypeService(IRepository<MilestoneType> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<MilestoneTypeDto>> GetListAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Libelle)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<MilestoneTypeDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<MilestoneTypeDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<MilestoneTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(t => t.Id == id).ProjectToType<MilestoneTypeDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<MilestoneTypeDto> CreateAsync(MilestoneTypeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<MilestoneType>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<MilestoneTypeDto>();
    }
}
