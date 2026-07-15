using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Services;

/// <summary>
/// Référentiel administrable des KPI de tableau de bord (cahier des charges §30, §25.1/§25.2) —
/// même principe que MilestoneTypeService (Lot 4) : seule la définition du KPI est persistée,
/// jamais sa valeur (toujours calculée à la demande par ReportingService).
/// </summary>
public class DashboardKpiService(IRepository<DashboardKpi> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<DashboardKpiDto>> GetListAsync(
        PaginationQuery pagination, DashboardKpiCategory? category, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (category is not null)
        {
            query = query.Where(k => k.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(k => k.Ordre)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<DashboardKpiDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<DashboardKpiDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<DashboardKpiDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(k => k.Id == id).ProjectToType<DashboardKpiDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<DashboardKpiDto> CreateAsync(DashboardKpiCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<DashboardKpi>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<DashboardKpiDto>();
    }
}
