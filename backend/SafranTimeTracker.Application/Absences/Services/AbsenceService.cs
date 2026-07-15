using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Absences;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Absences.Services;

/// <summary>
/// Workflow de demande d'absence (cahier des charges §23.2-§23.3). Suppression physique interdite
/// (CLAUDE.md §7, Absence explicitement listée) : "supprimer un brouillon" (§23.2) se traduit par
/// une annulation (statut Annule), jamais une suppression de ligne.
/// </summary>
public class AbsenceService(IRepository<Absence> repository, IReadRepository<SettingsEntity> settingsRepository, ICurrentUser currentUser)
{
    public async Task<PagedResult<AbsenceDto>> GetListAsync(
        PaginationQuery pagination, Guid? resourceId, AbsenceStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (resourceId is not null)
        {
            query = query.Where(a => a.ResourceId == resourceId);
        }
        if (statut is not null)
        {
            query = query.Where(a => a.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.DateDebut)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<AbsenceDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<AbsenceDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<AbsenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(a => a.Id == id).ProjectToType<AbsenceDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<AbsenceDto> CreateAsync(AbsenceCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Absence>();
        entity.Id = Guid.NewGuid();
        entity.Statut = AbsenceStatus.Brouillon;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<AbsenceDto>();
    }

    /// <summary>Si le workflow de validation est désactivé (Settings.ActivationValidationAbsences),
    /// la soumission vaut validation immédiate (§23.3) : AvailabilityService n'a alors besoin de
    /// filtrer que sur Statut == Valide, sans connaître ce paramètre.</summary>
    public async Task<AbsenceDto?> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForTransitionAsync(id, AbsenceStatus.Brouillon, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var workflowActif = await settingsRepository.Query().Select(s => s.ActivationValidationAbsences).FirstAsync(cancellationToken);
        var now = DateTime.UtcNow;

        entity.Statut = workflowActif ? AbsenceStatus.Soumis : AbsenceStatus.Valide;
        entity.UpdatedAt = now;
        entity.UpdatedBy = currentUser.Identifier;
        if (!workflowActif)
        {
            entity.ValideParIdentifiant = currentUser.Identifier;
            entity.DateDecision = now;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return entity.Adapt<AbsenceDto>();
    }

    public async Task<AbsenceDto?> ValidateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetForTransitionAsync(id, AbsenceStatus.Soumis, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Statut = AbsenceStatus.Valide;
        entity.ValideParIdentifiant = currentUser.Identifier;
        entity.DateDecision = DateTime.UtcNow;
        entity.UpdatedAt = entity.DateDecision;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);
        return entity.Adapt<AbsenceDto>();
    }

    public async Task<AbsenceDto?> RefuseAsync(Guid id, AbsenceDecisionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetForTransitionAsync(id, AbsenceStatus.Soumis, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Statut = AbsenceStatus.Refuse;
        entity.ValideParIdentifiant = currentUser.Identifier;
        entity.DateDecision = DateTime.UtcNow;
        entity.Commentaire = request.Commentaire;
        entity.UpdatedAt = entity.DateDecision;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);
        return entity.Adapt<AbsenceDto>();
    }

    public async Task<AbsenceDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }
        if (entity.Statut is AbsenceStatus.Refuse or AbsenceStatus.Annule)
        {
            throw new BusinessConflictException("Une absence refusée ou déjà annulée ne peut pas être annulée à nouveau.");
        }

        entity.Statut = AbsenceStatus.Annule;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Identifier;

        await repository.SaveChangesAsync(cancellationToken);
        return entity.Adapt<AbsenceDto>();
    }

    private async Task<Absence?> GetForTransitionAsync(Guid id, AbsenceStatus expectedStatus, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }
        if (entity.Statut != expectedStatus)
        {
            throw new BusinessConflictException(
                $"Transition impossible : l'absence est au statut '{entity.Statut}', '{expectedStatus}' attendu (cahier des charges §23.1).");
        }
        return entity;
    }
}
