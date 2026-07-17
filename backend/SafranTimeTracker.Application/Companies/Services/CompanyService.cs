using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Application.Companies.Services;

/// <summary>Création et modification auditées (cahier des charges §28.3, Lot 6).</summary>
public class CompanyService(IRepository<Company> repository, AuditService auditService)
{
    public async Task<PagedResult<CompanyDto>> GetListAsync(
        PaginationQuery pagination, Guid? companyTypeId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (companyTypeId is not null)
        {
            query = query.Where(c => c.CompanyTypeId == companyTypeId);
        }
        if (statut is not null)
        {
            query = query.Where(c => c.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<CompanyDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<CompanyDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(c => c.Id == id).ProjectToType<CompanyDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<CompanyDto> CreateAsync(CompanyCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Company>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(Company), entity.Id, null, entity.Adapt<CompanyDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CompanyDto>();
    }

    public async Task<CompanyDto?> UpdateAsync(Guid id, CompanyUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<CompanyDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentActor.PlaceholderIdentifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(Company), id, oldValue, entity.Adapt<CompanyDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<CompanyDto>();
    }
}
