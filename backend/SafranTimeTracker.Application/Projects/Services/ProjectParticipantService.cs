using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>
/// Cahier des charges §17.2. Société applicable/TJM ne sont jamais stockés : calculés à la date du
/// jour via FinancialCalculationService (Lot 2, réutilisé tel quel — HeuresSaisies=0 n'affecte pas
/// la résolution du TJM/de la société applicable, seul le coût s'annule).
/// </summary>
public class ProjectParticipantService(
    IRepository<ProjectParticipant> repository,
    FinancialCalculationService financialCalculationService,
    ICurrentUser currentUser)
{
    public async Task<PagedResult<ProjectParticipantDto>> GetListAsync(
        Guid projectId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = repository.Query().Where(p => p.ProjectId == projectId);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(p => p.DateDebut)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var hasFinancialAccess = currentUser.HasPermission(PermissionCodes.FinancialDataView);
        var items = new List<ProjectParticipantDto>(entities.Count);
        foreach (var entity in entities)
        {
            items.Add(await ToDtoAsync(entity, hasFinancialAccess, cancellationToken));
        }

        return new PagedResult<ProjectParticipantDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public async Task<ProjectParticipantDto> CreateAsync(
        Guid projectId, ProjectParticipantCreateRequest request, CancellationToken cancellationToken = default)
    {
        var existingPeriods = await repository.Query()
            .Where(p => p.ProjectId == projectId && p.ResourceId == request.ResourceId && p.Statut == ReferentialStatus.Actif)
            .Select(p => new { p.DateDebut, p.DateFin })
            .ToListAsync(cancellationToken);

        if (existingPeriods.Any(p => DateRangeOverlap.Overlaps(p.DateDebut, p.DateFin, request.DateDebut, request.DateFin)))
        {
            throw new BusinessConflictException(
                "Cette ressource participe déjà à ce projet sur une période qui chevauche celle indiquée (cahier des charges §17.2).");
        }

        var entity = request.Adapt<ProjectParticipant>();
        entity.Id = Guid.NewGuid();
        entity.ProjectId = projectId;
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(entity, currentUser.HasPermission(PermissionCodes.FinancialDataView), cancellationToken);
    }

    public async Task<ProjectParticipantDto?> RemoveAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(participantId, cancellationToken);
        if (entity is null || entity.ProjectId != projectId)
        {
            return null;
        }

        entity.Statut = ReferentialStatus.Inactif;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(entity, currentUser.HasPermission(PermissionCodes.FinancialDataView), cancellationToken);
    }

    private async Task<ProjectParticipantDto> ToDtoAsync(ProjectParticipant entity, bool hasFinancialAccess, CancellationToken cancellationToken)
    {
        var dto = entity.Adapt<ProjectParticipantDto>();
        if (!hasFinancialAccess)
        {
            dto.FinancialSummary = null;
            return dto;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await financialCalculationService.CalculateAsync(
            new FinancialCalculationRequest { ResourceId = entity.ResourceId, Date = today, HeuresSaisies = 0m },
            cancellationToken);

        dto.FinancialSummary = new ProjectParticipantFinancialSummaryDto
        {
            CompanyIdApplicable = result.CompanyId,
            TjmPersonneApplicable = result.DailyRatePersonne,
            TjmContratApplicable = result.DailyRateContrat
        };
        return dto;
    }
}
