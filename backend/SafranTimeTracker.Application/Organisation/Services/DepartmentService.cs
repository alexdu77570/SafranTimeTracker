using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Application.Organisation.Services;

public class DepartmentService(IRepository<Department> repository)
{
    public async Task<PagedResult<DepartmentDto>> GetListAsync(
        PaginationQuery pagination, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (statut is not null)
        {
            query = query.Where(d => d.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<DepartmentDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(d => d.Id == id).ProjectToType<DepartmentDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<DepartmentDto> CreateAsync(DepartmentCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Department>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<DepartmentDto>();
    }
}
