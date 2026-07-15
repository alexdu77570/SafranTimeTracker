using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Application.Organisation.Services;

public class TeamService(IRepository<Team> repository)
{
    public async Task<PagedResult<TeamDto>> GetListAsync(
        PaginationQuery pagination, Guid? serviceId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (serviceId is not null)
        {
            query = query.Where(t => t.ServiceId == serviceId);
        }
        if (statut is not null)
        {
            query = query.Where(t => t.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<TeamDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<TeamDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<TeamDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(t => t.Id == id).ProjectToType<TeamDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<TeamDto> CreateAsync(TeamCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Team>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<TeamDto>();
    }
}
