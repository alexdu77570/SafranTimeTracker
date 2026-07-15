using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Application.Milestones.Services;

/// <summary>Cahier des charges §24. "En retard" (§24.2) est dérivé à la lecture, jamais stocké
/// (voir MilestoneDto.EstEnRetard).</summary>
public class MilestoneService(IRepository<Milestone> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<MilestoneDto>> GetListAsync(
        PaginationQuery pagination, Guid? projectId, Guid? responsableId, MilestoneStatus? statut, bool? enRetard,
        CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (projectId is not null) query = query.Where(m => m.ProjectId == projectId);
        if (responsableId is not null) query = query.Where(m => m.ResponsableId == responsableId);
        if (statut is not null) query = query.Where(m => m.Statut == statut);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(m => m.DatePrevue)
            .ToListAsync(cancellationToken);

        var items = entities.Select(ToDto).AsEnumerable();
        if (enRetard is not null)
        {
            items = items.Where(m => m.EstEnRetard == enRetard);
        }

        var filtered = items.ToList();
        var pageItems = filtered
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return new PagedResult<MilestoneDto>
        {
            Items = pageItems,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = enRetard is null ? totalCount : filtered.Count
        };
    }

    public async Task<MilestoneDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<MilestoneDto> CreateAsync(MilestoneCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Milestone>();
        entity.Id = Guid.NewGuid();
        entity.Statut = MilestoneStatus.AVenir;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<MilestoneDto?> UpdateAsync(Guid id, MilestoneUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    private static MilestoneDto ToDto(Milestone entity)
    {
        var dto = entity.Adapt<MilestoneDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        dto.EstEnRetard = entity.DatePrevue < today
            && entity.Statut is not (MilestoneStatus.Termine or MilestoneStatus.Annule);
        return dto;
    }
}
