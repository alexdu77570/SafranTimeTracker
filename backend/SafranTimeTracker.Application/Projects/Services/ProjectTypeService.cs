using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>Référentiel des types de projet (docs/BACKLOG_METIER.md §7, Lot 8) : création/modification auditées.</summary>
public class ProjectTypeService(IRepository<ProjectType> repository, ICurrentUser currentUser, AuditService auditService)
{
    public async Task<PagedResult<ProjectTypeDto>> GetListAsync(
        PaginationQuery pagination, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (statut is not null)
        {
            query = query.Where(t => t.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Libelle)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ProjectTypeDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectTypeDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ProjectTypeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(t => t.Id == id).ProjectToType<ProjectTypeDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ProjectTypeDto> CreateAsync(ProjectTypeCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<ProjectType>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(ProjectType), entity.Id, null, entity.Adapt<ProjectTypeDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ProjectTypeDto>();
    }

    public async Task<ProjectTypeDto?> UpdateAsync(Guid id, ProjectTypeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<ProjectTypeDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(ProjectType), id, oldValue, entity.Adapt<ProjectTypeDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ProjectTypeDto>();
    }
}
