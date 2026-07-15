using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Applications.Services;

/// <summary>
/// Renommé par rapport au cahier des charges §31 ("ApplicationService") pour éviter la
/// collision avec le namespace de couche SafranTimeTracker.Application (voir CLAUDE.md §6).
/// </summary>
public class ApplicationReferenceService(IRepository<ApplicationReference> repository)
{
    public async Task<PagedResult<ApplicationReferenceDto>> GetListAsync(
        PaginationQuery pagination, Guid? serviceId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (serviceId is not null)
        {
            query = query.Where(a => a.ServiceId == serviceId);
        }
        if (statut is not null)
        {
            query = query.Where(a => a.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ApplicationReferenceDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationReferenceDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ApplicationReferenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(a => a.Id == id).ProjectToType<ApplicationReferenceDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ApplicationReferenceDto> CreateAsync(ApplicationReferenceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<ApplicationReference>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ApplicationReferenceDto>();
    }
}
