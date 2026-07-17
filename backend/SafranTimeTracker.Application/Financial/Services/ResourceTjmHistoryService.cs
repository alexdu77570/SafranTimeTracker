using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>Cahier des charges §11. Le chevauchement de périodes est un conflit métier (409),
/// pas une erreur de format : il dépend de l'état existant, pas de la requête seule. Création et
/// modification auditées (§28.3, Lot 6).</summary>
public class ResourceTjmHistoryService(IRepository<ResourceTjmHistory> repository, AuditService auditService, ICurrentUser currentUser)
{
    public async Task<PagedResult<ResourceTjmHistoryDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (resourceId is not null)
        {
            query = query.Where(h => h.ResourceId == resourceId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(h => h.StartDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ResourceTjmHistoryDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ResourceTjmHistoryDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ResourceTjmHistoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(h => h.Id == id).ProjectToType<ResourceTjmHistoryDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ResourceTjmHistoryDto> CreateAsync(ResourceTjmHistoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNoOverlapAsync(request.ResourceId, request.StartDate, request.EndDate, excludeId: null, cancellationToken);

        var entity = request.Adapt<ResourceTjmHistory>();
        entity.Id = Guid.NewGuid();
        entity.Status = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(ResourceTjmHistory), entity.Id, null, entity.Adapt<ResourceTjmHistoryDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceTjmHistoryDto>();
    }

    public async Task<ResourceTjmHistoryDto?> UpdateAsync(Guid id, ResourceTjmHistoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await EnsureNoOverlapAsync(entity.ResourceId, request.StartDate, request.EndDate, excludeId: id, cancellationToken);

        var oldValue = entity.Adapt<ResourceTjmHistoryDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;
        entity.ConcurrencyStamp = Guid.NewGuid();

        await auditService.RecordAsync(
            AuditActions.Update, nameof(ResourceTjmHistory), id, oldValue, entity.Adapt<ResourceTjmHistoryDto>(),
            request.Reason, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceTjmHistoryDto>();
    }

    private async Task EnsureNoOverlapAsync(
        Guid resourceId, DateOnly startDate, DateOnly? endDate, Guid? excludeId, CancellationToken cancellationToken)
    {
        var existingPeriods = await repository.Query()
            .Where(h => h.ResourceId == resourceId && h.Status == ReferentialStatus.Actif && (excludeId == null || h.Id != excludeId))
            .Select(h => new { h.StartDate, h.EndDate })
            .ToListAsync(cancellationToken);

        if (existingPeriods.Any(h => DateRangeOverlap.Overlaps(h.StartDate, h.EndDate, startDate, endDate)))
        {
            throw new BusinessConflictException(
                "Une période TJM active existe déjà pour cette ressource sur cette plage de dates (cahier des charges §11.3).");
        }
    }
}
