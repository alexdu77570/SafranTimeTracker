using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Resources.Services;

public class ResourceService(IRepository<Resource> repository)
{
    public async Task<PagedResult<ResourceDto>> GetListAsync(
        PaginationQuery pagination, Guid? departmentId, Guid? serviceId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (departmentId is not null)
        {
            query = query.Where(r => r.DepartmentId == departmentId);
        }
        if (serviceId is not null)
        {
            query = query.Where(r => r.ServiceId == serviceId);
        }
        if (statut is not null)
        {
            query = query.Where(r => r.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Nom).ThenBy(r => r.Prenom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ResourceDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ResourceDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(r => r.Id == id).ProjectToType<ResourceDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ResourceDto> CreateAsync(ResourceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Resource>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;
        entity.OperationalRoles = request.OperationalRoleIds
            .Select(roleId => new ResourceOperationalRole { ResourceId = entity.Id, OperationalRoleId = roleId })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceDto>();
    }
}
