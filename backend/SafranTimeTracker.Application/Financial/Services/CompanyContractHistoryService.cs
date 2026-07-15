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

/// <summary>Cahier des charges §12.3-§12.4. Mêmes règles d'intégrité que ResourceTjmHistory
/// (docs/DATABASE.md §5), chevauchement traduit en 409 (CLAUDE.md §12).</summary>
public class CompanyContractHistoryService(IRepository<CompanyContractHistory> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<CompanyContractHistoryDto>> GetListAsync(
        PaginationQuery pagination, Guid? companyId, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (companyId is not null)
        {
            query = query.Where(h => h.CompanyId == companyId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(h => h.StartDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<CompanyContractHistoryDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<CompanyContractHistoryDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<CompanyContractHistoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(h => h.Id == id).ProjectToType<CompanyContractHistoryDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<CompanyContractHistoryDto> CreateAsync(CompanyContractHistoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNoOverlapAsync(request.CompanyId, request.StartDate, request.EndDate, excludeId: null, cancellationToken);

        var entity = request.Adapt<CompanyContractHistory>();
        entity.Id = Guid.NewGuid();
        entity.Status = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CompanyContractHistoryDto>();
    }

    public async Task<CompanyContractHistoryDto?> UpdateAsync(Guid id, CompanyContractHistoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await EnsureNoOverlapAsync(entity.CompanyId, request.StartDate, request.EndDate, excludeId: id, cancellationToken);

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;
        entity.ConcurrencyStamp = Guid.NewGuid();

        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CompanyContractHistoryDto>();
    }

    private async Task EnsureNoOverlapAsync(
        Guid companyId, DateOnly startDate, DateOnly? endDate, Guid? excludeId, CancellationToken cancellationToken)
    {
        var existingPeriods = await repository.Query()
            .Where(h => h.CompanyId == companyId && h.Status == ReferentialStatus.Actif && (excludeId == null || h.Id != excludeId))
            .Select(h => new { h.StartDate, h.EndDate })
            .ToListAsync(cancellationToken);

        if (existingPeriods.Any(h => DateRangeOverlap.Overlaps(h.StartDate, h.EndDate, startDate, endDate)))
        {
            throw new BusinessConflictException(
                "Un contrat actif existe déjà pour cette société sur cette plage de dates (cahier des charges §12.4).");
        }
    }
}
