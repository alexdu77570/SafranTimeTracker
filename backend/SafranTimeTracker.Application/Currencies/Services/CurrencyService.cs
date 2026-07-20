using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Currencies.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Currencies;

namespace SafranTimeTracker.Application.Currencies.Services;

/// <summary>
/// Référentiel des devises (docs/BACKLOG_METIER.md §9, Lot 8) : référentiel de consultation,
/// création/modification auditées. Aucun impact sur FinancialCalculationService (voir BACKLOG_METIER.md).
/// </summary>
public class CurrencyService(IRepository<Currency> repository, ICurrentUser currentUser, AuditService auditService)
{
    public async Task<PagedResult<CurrencyDto>> GetListAsync(
        PaginationQuery pagination, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (statut is not null)
        {
            query = query.Where(c => c.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.CodeIso)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<CurrencyDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<CurrencyDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<CurrencyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(c => c.Id == id).ProjectToType<CurrencyDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<CurrencyDto> CreateAsync(CurrencyCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Currency>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(Currency), entity.Id, null, entity.Adapt<CurrencyDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CurrencyDto>();
    }

    public async Task<CurrencyDto?> UpdateAsync(Guid id, CurrencyUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<CurrencyDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(Currency), id, oldValue, entity.Adapt<CurrencyDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CurrencyDto>();
    }
}
