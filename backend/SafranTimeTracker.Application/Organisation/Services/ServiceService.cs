using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Common;
using ServiceEntity = SafranTimeTracker.Domain.Organisation.Service;

namespace SafranTimeTracker.Application.Organisation.Services;

public class ServiceService(IRepository<ServiceEntity> repository)
{
    public async Task<PagedResult<ServiceDto>> GetListAsync(
        PaginationQuery pagination, Guid? departmentId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (departmentId is not null)
        {
            query = query.Where(s => s.DepartmentId == departmentId);
        }
        if (statut is not null)
        {
            query = query.Where(s => s.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ServiceDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(s => s.Id == id).ProjectToType<ServiceDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ServiceDto> CreateAsync(ServiceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<ServiceEntity>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ServiceDto>();
    }
}
