using Mapster;
using Microsoft.EntityFrameworkCore;
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
/// FINANCIAL_DATA_VIEW (CLAUDE.md §13), projection faite ici, jamais a posteriori.
/// </summary>
public class TimeEntryService(
    IRepository<TimeEntry> repository,
    IRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    IReadRepository<Resource> resourceRepository,
    IReadRepository<Order> orderRepository,
    IReadRepository<SettingsEntity> settingsRepository,
    FinancialCalculationService financialCalculationService,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<TimeEntryDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
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

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);

        await ValorizeAsync(entity, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task EnsureBusinessRulesAsync(Guid resourceId, Guid? orderId, DateOnly date, CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId, cancellationToken);
        if (resource is null || resource.Statut != ReferentialStatus.Actif)
        {
            throw new BusinessConflictException("Impossible de saisir du temps sur une ressource inactive (cahier des charges §19.4).");
        }

        var delaiModificationJours = await settingsRepository.Query()
            .Select(s => s.DelaiModificationTempsJours)
            .FirstAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today.DayNumber - date.DayNumber > delaiModificationJours)
        {
            throw new BusinessConflictException(
                $"La période est clôturée : la saisie n'est plus modifiable au-delà de {delaiModificationJours} jour(s) (cahier des charges §19.4).");
        }

        if (orderId is not null)
        {
            var order = await orderRepository.GetByIdAsync(orderId.Value, cancellationToken);
            var applicableCompanyId = await financialCalculationService.GetApplicableCompanyIdAsync(resourceId, date, cancellationToken);
            if (order is not null && applicableCompanyId is not null && order.CompanyId != applicableCompanyId)
            {
                throw new BusinessConflictException(
                    "La commande n'est pas compatible avec la société de la ressource à la date de la saisie (cahier des charges §13.4).");
            }
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
            ApplyResult(snapshot, result, now);
            await snapshotRepository.AddAsync(snapshot, cancellationToken);
        }
        else
        {
            ApplyResult(snapshot, result, now);
            snapshot.UpdatedAt = now;
            snapshot.UpdatedBy = currentUser.Identifier;
        }

        await snapshotRepository.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyResult(TimeEntryFinancialSnapshot snapshot, FinancialCalculationResultDto result, DateTime calculationDate)
    {
        snapshot.TjmPersonneSnapshot = result.DailyRatePersonne;
        snapshot.SourceTjmPersonne = result.SourceTjmPersonne;
        snapshot.ResourceTjmHistoryId = result.ResourceTjmHistoryId;
        snapshot.TjmContratSnapshot = result.DailyRateContrat;
        snapshot.SourceContrat = result.SourceContrat;
        snapshot.CompanyContractHistoryId = result.CompanyContractHistoryId;
        snapshot.CompanyIdSnapshot = result.CompanyId;
        snapshot.CoutReelCalcule = result.CoutReel;
        snapshot.CoutContratCalcule = result.CoutContractuel;
        snapshot.DifferentielCalcule = result.Differentiel;
        snapshot.CalculationDate = calculationDate;
        snapshot.CalculationStatus = result.ValuationStatus;
    }

    private static TimeEntryDto ToDto(TimeEntry entity, bool hasFinancialAccess)
    {
        var dto = entity.Adapt<TimeEntryDto>();
        dto.FinancialSnapshot = hasFinancialAccess ? entity.FinancialSnapshot?.Adapt<TimeEntryFinancialSnapshotDto>() : null;
        return dto;
    }
}
