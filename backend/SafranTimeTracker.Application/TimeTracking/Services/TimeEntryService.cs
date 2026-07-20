using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Time;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.TimeTracking.Services;

/// <summary>
/// Orchestration de la saisie de temps (cahier des charges §19). Toute création ou modification
/// autorisée revalorise la saisie via FinancialCalculationService (§19.5) — la seule source de
/// calcul, jamais dupliquée ici. Le sous-objet financier du DTO retourné est omis sans
/// FINANCIAL_DATA_VIEW (CLAUDE.md §13), projection faite ici, jamais a posteriori. Création,
/// modification et suppression logique sont auditées (§28.3, Lot 6).
/// </summary>
public class TimeEntryService(
    IRepository<TimeEntry> repository,
    IRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    IReadRepository<Resource> resourceRepository,
    IReadRepository<Order> orderRepository,
    IReadRepository<OrderStatus> orderStatusRepository,
    IReadRepository<SettingsEntity> settingsRepository,
    FinancialCalculationService financialCalculationService,
    AuditService auditService,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<TimeEntryDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, DateOnly? from, DateOnly? to,
        Guid? activityTypeId = null, Guid? projectId = null, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (resourceId is not null)
        {
            query = query.Where(t => t.ResourceId == resourceId);
        }
        if (from is not null)
        {
            query = query.Where(t => t.Date >= from);
        }
        if (to is not null)
        {
            query = query.Where(t => t.Date <= to);
        }
        // Filtres serveur additionnels (cahier des charges §19.4, docs/BACKLOG_METIER.md §10) :
        // ActivityTypeId/ProjectId/OrderId existent déjà sur l'entité depuis les Lots 3/4, seul le
        // filtrage était manquant.
        if (activityTypeId is not null)
        {
            query = query.Where(t => t.ActivityTypeId == activityTypeId);
        }
        if (projectId is not null)
        {
            query = query.Where(t => t.ProjectId == projectId);
        }
        if (orderId is not null)
        {
            query = query.Where(t => t.OrderId == orderId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Include(t => t.FinancialSnapshot)
            .OrderByDescending(t => t.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var hasFinancialAccess = currentUser.HasPermission(PermissionCodes.FinancialDataView);
        var items = entities.Select(e => ToDto(e, hasFinancialAccess)).ToList();

        return new PagedResult<TimeEntryDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<TimeEntryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.Query()
            .Include(t => t.FinancialSnapshot)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity is null ? null : ToDto(entity, currentUser.HasPermission(PermissionCodes.FinancialDataView));
    }

    public async Task<TimeEntryDto> CreateAsync(TimeEntryCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureBusinessRulesAsync(request.ResourceId, request.OrderId, request.Date, cancellationToken);

        var entity = request.Adapt<TimeEntry>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await auditService.RecordAsync(
            AuditActions.Create, nameof(TimeEntry), entity.Id, null, ToDto(entity, hasFinancialAccess: false), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await ValorizeAsync(entity, cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<TimeEntryDto?> UpdateAsync(Guid id, TimeEntryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await EnsureBusinessRulesAsync(entity.ResourceId, request.OrderId, request.Date, cancellationToken);

        var oldValue = ToDto(entity, hasFinancialAccess: false);
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(TimeEntry), id, oldValue, ToDto(entity, hasFinancialAccess: false), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await ValorizeAsync(entity, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>§28.3 "suppression logique d'une saisie" : statut plutôt que suppression physique
    /// (CLAUDE.md §7), même règle de période close que <see cref="UpdateAsync"/> (§19.4).</summary>
    public async Task<TimeEntryDto?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }
        if (entity.Statut == ReferentialStatus.Inactif)
        {
            throw new BusinessConflictException("Cette saisie est déjà supprimée.");
        }

        await EnsurePeriodNotClosedAsync(entity.Date, cancellationToken);

        var oldValue = ToDto(entity, hasFinancialAccess: false);
        entity.Statut = ReferentialStatus.Inactif;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await auditService.RecordAsync(AuditActions.LogicalDelete, nameof(TimeEntry), id, oldValue, null, cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<TimeEntryDto>();
    }

    private async Task EnsureBusinessRulesAsync(Guid resourceId, Guid? orderId, DateOnly date, CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId, cancellationToken);
        if (resource is null || resource.Statut != ReferentialStatus.Actif)
        {
            throw new BusinessConflictException("Impossible de saisir du temps sur une ressource inactive (cahier des charges §19.4).");
        }

        await EnsurePeriodNotClosedAsync(date, cancellationToken);

        if (orderId is not null)
        {
            var order = await orderRepository.GetByIdAsync(orderId.Value, cancellationToken);
            var applicableCompanyId = await financialCalculationService.GetApplicableCompanyIdAsync(resourceId, date, cancellationToken);
            if (order is not null && applicableCompanyId is not null && order.CompanyId != applicableCompanyId)
            {
                throw new BusinessConflictException(
                    "La commande n'est pas compatible avec la société de la ressource à la date de la saisie (cahier des charges §13.4).");
            }

            // §13.4 : "une saisie liée à une commande clôturée doit être bloquée, sauf droit de
            // correction" — TIME_ENTRY_CORRECTION (Lot 5) est ce droit explicite.
            if (order is not null && !currentUser.HasPermission(PermissionCodes.TimeEntryCorrection))
            {
                var status = await orderStatusRepository.GetByIdAsync(order.StatusId, cancellationToken);
                if (status?.Code == "CLOTUREE")
                {
                    throw new BusinessConflictException(
                        "La commande est clôturée : la saisie est bloquée, sauf droit de correction (cahier des charges §13.4).");
                }
            }
        }
    }

    private async Task EnsurePeriodNotClosedAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var delaiModificationJours = await settingsRepository.Query()
            .Select(s => s.DelaiModificationTempsJours)
            .FirstAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today.DayNumber - date.DayNumber > delaiModificationJours)
        {
            throw new BusinessConflictException(
                $"La période est clôturée : la saisie n'est plus modifiable au-delà de {delaiModificationJours} jour(s) (cahier des charges §19.4).");
        }
    }

    private async Task ValorizeAsync(TimeEntry entity, CancellationToken cancellationToken)
    {
        var result = await financialCalculationService.CalculateAsync(
            new FinancialCalculationRequest { ResourceId = entity.ResourceId, Date = entity.Date, HeuresSaisies = entity.DureeHeures },
            cancellationToken);

        // Clé partagée avec TimeEntry (Id == TimeEntryId) : GetByIdAsync renvoie une entité suivie
        // par le contexte, contrairement à Query() (AsNoTracking, cf. EfRepository) — une mutation
        // en place sur un résultat de Query() ne serait pas persistée par SaveChangesAsync.
        var snapshot = await snapshotRepository.GetByIdAsync(entity.Id, cancellationToken);
        var now = DateTime.UtcNow;

        if (snapshot is null)
        {
            snapshot = new TimeEntryFinancialSnapshot
            {
                Id = entity.Id,
                TimeEntryId = entity.Id,
                CreatedAt = now,
                CreatedBy = currentUser.Identifier
            };
            FinancialCalculationService.ApplyToSnapshot(snapshot, result, now);
            await snapshotRepository.AddAsync(snapshot, cancellationToken);
        }
        else
        {
            FinancialCalculationService.ApplyToSnapshot(snapshot, result, now);
            snapshot.UpdatedAt = now;
            snapshot.UpdatedBy = currentUser.Identifier;
        }

        await snapshotRepository.SaveChangesAsync(cancellationToken);
    }

    private static TimeEntryDto ToDto(TimeEntry entity, bool hasFinancialAccess)
    {
        var dto = entity.Adapt<TimeEntryDto>();
        dto.FinancialSnapshot = hasFinancialAccess ? entity.FinancialSnapshot?.Adapt<TimeEntryFinancialSnapshotDto>() : null;
        return dto;
    }
}
