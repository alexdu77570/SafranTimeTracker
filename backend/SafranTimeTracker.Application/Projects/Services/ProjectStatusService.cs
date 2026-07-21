using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>
/// Référentiel de statuts de projet (cahier des charges §16.2, §30) : Actif, Suspendu, Terminé,
/// Archivé — lecture seule (écart constaté à l'ouverture du Lot 10, même nature que
/// CompanyType/Role au Lot 8, corrigée ici plutôt que contournée par un second
/// `knownReferentials.ts`, décision validée par l'utilisateur). Aucune permission requise, même
/// principe d'ouverture que les autres référentiels non financiers (`departments`, `activity-types`).
/// </summary>
public class ProjectStatusService(IReadRepository<ProjectStatus> repository)
{
    public async Task<PagedResult<ProjectStatusDto>> GetListAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(s => s.Ordre)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ProjectStatusDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectStatusDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ProjectStatusDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(s => s.Id == id).ProjectToType<ProjectStatusDto>().FirstOrDefaultAsync(cancellationToken);
}
