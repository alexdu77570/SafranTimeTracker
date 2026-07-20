using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Clients.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Clients;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Clients.Services;

/// <summary>Référentiel des clients (docs/BACKLOG_METIER.md §6, Lot 8) : création/modification auditées.</summary>
public class ClientService(IRepository<Client> repository, ICurrentUser currentUser, AuditService auditService)
{
    public async Task<PagedResult<ClientDto>> GetListAsync(
        PaginationQuery pagination, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (statut is not null)
        {
            query = query.Where(c => c.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.Nom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ClientDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(c => c.Id == id).ProjectToType<ClientDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<ClientDto> CreateAsync(ClientCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Client>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(Client), entity.Id, null, entity.Adapt<ClientDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ClientDto>();
    }

    public async Task<ClientDto?> UpdateAsync(Guid id, ClientUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var oldValue = entity.Adapt<ClientDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(Client), id, oldValue, entity.Adapt<ClientDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<ClientDto>();
    }
}
