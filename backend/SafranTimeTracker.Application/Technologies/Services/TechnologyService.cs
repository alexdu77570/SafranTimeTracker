using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Technologies.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Technologies;

namespace SafranTimeTracker.Application.Technologies.Services;

/// <summary>
/// Référentiel des technologies (docs/BACKLOG_METIER.md §5, Lot 8) : création/modification
/// auditées. Les liaisons Application/Ressource sont gérées par suppression puis recréation
/// explicite des lignes de jointure (IRepository&lt;T&gt;.RemoveAsync par "stub" ne portant que
/// la clé, même principe que UserService.RevokePermissionAsync, CLAUDE.md §11) plutôt que par
/// réaffectation de la collection de navigation : IRepository&lt;T&gt;.Query() est non suivi
/// (AsNoTracking), donc une entité récupérée par Query().Include(...) puis mutée ne serait
/// jamais persistée par SaveChangesAsync.
/// </summary>
public class TechnologyService(
    IRepository<Technology> repository,
    IRepository<ApplicationTechnology> applicationTechnologyRepository,
    IRepository<ResourceTechnology> resourceTechnologyRepository,
    ICurrentUser currentUser,
    AuditService auditService)
{
    public async Task<PagedResult<TechnologyDto>> GetListAsync(
        PaginationQuery pagination, Guid? applicationId, Guid? resourceId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query().Include(t => t.Applications).Include(t => t.Resources).AsQueryable();
        if (applicationId is not null)
        {
            query = query.Where(t => t.Applications.Any(a => a.ApplicationId == applicationId));
        }
        if (resourceId is not null)
        {
            query = query.Where(t => t.Resources.Any(r => r.ResourceId == resourceId));
        }
        if (statut is not null)
        {
            query = query.Where(t => t.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Libelle)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TechnologyDto>
        {
            Items = items.Adapt<List<TechnologyDto>>(), Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount,
        };
    }

    public async Task<TechnologyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.Query().Include(t => t.Applications).Include(t => t.Resources)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return entity?.Adapt<TechnologyDto>();
    }

    public async Task<TechnologyDto> CreateAsync(TechnologyCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Technology>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;
        // Entité nouvelle (jamais persistée) : la réaffectation de la collection avant AddAsync
        // est ici sûre, tout le graphe est marqué Added lors de l'ajout de l'agrégat racine.
        entity.Applications = request.ApplicationIds
            .Select(applicationId => new ApplicationTechnology { TechnologyId = entity.Id, ApplicationId = applicationId })
            .ToList();
        entity.Resources = request.ResourceIds
            .Select(resourceId => new ResourceTechnology { TechnologyId = entity.Id, ResourceId = resourceId })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(Technology), entity.Id, null, entity.Adapt<TechnologyDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<TechnologyDto?> UpdateAsync(Guid id, TechnologyUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var existingApplicationIds = await applicationTechnologyRepository.Query()
            .Where(a => a.TechnologyId == id).Select(a => a.ApplicationId).ToListAsync(cancellationToken);
        var existingResourceIds = await resourceTechnologyRepository.Query()
            .Where(r => r.TechnologyId == id).Select(r => r.ResourceId).ToListAsync(cancellationToken);

        var oldValue = new TechnologyDto
        {
            Id = id, Code = entity.Code, Libelle = entity.Libelle, Statut = entity.Statut,
            ApplicationIds = existingApplicationIds, ResourceIds = existingResourceIds,
        };

        foreach (var applicationId in existingApplicationIds)
        {
            await applicationTechnologyRepository.RemoveAsync(
                new ApplicationTechnology { ApplicationId = applicationId, TechnologyId = id }, cancellationToken);
        }
        foreach (var resourceId in existingResourceIds)
        {
            await resourceTechnologyRepository.RemoveAsync(
                new ResourceTechnology { ResourceId = resourceId, TechnologyId = id }, cancellationToken);
        }
        foreach (var applicationId in request.ApplicationIds)
        {
            await applicationTechnologyRepository.AddAsync(
                new ApplicationTechnology { ApplicationId = applicationId, TechnologyId = id }, cancellationToken);
        }
        foreach (var resourceId in request.ResourceIds)
        {
            await resourceTechnologyRepository.AddAsync(
                new ResourceTechnology { ResourceId = resourceId, TechnologyId = id }, cancellationToken);
        }

        entity.Libelle = request.Libelle;
        entity.Statut = request.Statut;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        var newValue = new TechnologyDto
        {
            Id = id, Code = entity.Code, Libelle = entity.Libelle, Statut = entity.Statut,
            ApplicationIds = request.ApplicationIds, ResourceIds = request.ResourceIds,
        };
        await auditService.RecordAsync(AuditActions.Update, nameof(Technology), id, oldValue, newValue, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }
}
