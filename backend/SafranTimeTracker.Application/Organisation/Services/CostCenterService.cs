using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Application.Organisation.Services;

/// <summary>Référentiel des centres de coûts (docs/BACKLOG_METIER.md §8, Lot 8) : création/modification auditées.</summary>
public class CostCenterService(IRepository<CostCenter> repository, ICurrentUser currentUser, AuditService auditService)
{
    public async Task<PagedResult<CostCenterDto>> GetListAsync(
        PaginationQuery pagination, Guid? departmentId, Guid? serviceId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (departmentId is not null)
        {
            query = query.Where(c => c.DepartmentId == departmentId);
        }
        if (serviceId is not null)
        {
            query = query.Where(c => c.ServiceId == serviceId);
        }
        if (statut is not null)
        {
            query = query.Where(c => c.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Libelle)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<CostCenterDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<CostCenterDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<CostCenterDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(c => c.Id == id).ProjectToType<CostCenterDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<CostCenterDto> CreateAsync(CostCenterCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<CostCenter>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(CostCenter), entity.Id, null, entity.Adapt<CostCenterDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CostCenterDto>();
    }

    public async Task<CostCenterDto?> UpdateAsync(Guid id, CostCenterUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<CostCenterDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(CostCenter), id, oldValue, entity.Adapt<CostCenterDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CostCenterDto>();
    }
}
