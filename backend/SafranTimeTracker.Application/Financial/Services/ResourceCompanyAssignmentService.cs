using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>Cahier des charges §12.2. Source de vérité utilisée par FinancialCalculationService
/// pour déterminer la société applicable à une date — distincte de Resource.CompanyId (Lot 1),
/// pointeur d'affichage non historisé.</summary>
public class ResourceCompanyAssignmentService(IRepository<ResourceCompanyAssignment> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<ResourceCompanyAssignmentDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, Guid? companyId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (resourceId is not null)
        {
            query = query.Where(a => a.ResourceId == resourceId);
        }
        if (companyId is not null)
        {
            query = query.Where(a => a.CompanyId == companyId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.StartDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ResourceCompanyAssignmentDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ResourceCompanyAssignmentDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ResourceCompanyAssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(a => a.Id == id).ProjectToType<ResourceCompanyAssignmentDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ResourceCompanyAssignmentDto> CreateAsync(ResourceCompanyAssignmentCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNoOverlapAsync(request.ResourceId, request.StartDate, request.EndDate, excludeId: null, cancellationToken);

        var entity = request.Adapt<ResourceCompanyAssignment>();
        entity.Id = Guid.NewGuid();
        entity.Status = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceCompanyAssignmentDto>();
    }

    public async Task<ResourceCompanyAssignmentDto?> UpdateAsync(Guid id, ResourceCompanyAssignmentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await EnsureNoOverlapAsync(entity.ResourceId, request.StartDate, request.EndDate, excludeId: id, cancellationToken);

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ResourceCompanyAssignmentDto>();
    }

    private async Task EnsureNoOverlapAsync(
        Guid resourceId, DateOnly startDate, DateOnly? endDate, Guid? excludeId, CancellationToken cancellationToken)
    {
        var existingPeriods = await repository.Query()
            .Where(a => a.ResourceId == resourceId && a.Status == ReferentialStatus.Actif && (excludeId == null || a.Id != excludeId))
            .Select(a => new { a.StartDate, a.EndDate })
            .ToListAsync(cancellationToken);

        if (existingPeriods.Any(a => DateRangeOverlap.Overlaps(a.StartDate, a.EndDate, startDate, endDate)))
        {
            throw new BusinessConflictException(
                "Un rattachement société actif existe déjà pour cette ressource sur cette plage de dates (cahier des charges §12.2).");
        }
    }
}
