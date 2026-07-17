using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Auditing;

namespace SafranTimeTracker.Application.Audit.Services;

/// <summary>Consultation du journal d'audit (§28.1 onglet "Audit", §28.3 "jamais modifiable
/// depuis l'interface standard" : ce service n'expose donc aucune écriture, seul
/// <see cref="AuditService"/> écrit.</summary>
public class AuditLogService(IReadRepository<AuditLog> repository)
{
    public async Task<PagedResult<AuditLogDto>> GetListAsync(
        PaginationQuery pagination, string? author, string? entityType, string? action,
        DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(a => a.Author == author);
        }
        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }
        if (from is not null)
        {
            query = query.Where(a => a.Timestamp >= from);
        }
        if (to is not null)
        {
            query = query.Where(a => a.Timestamp <= to);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<AuditLogDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity?.Adapt<AuditLogDto>();
    }
}
