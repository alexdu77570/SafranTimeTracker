using Mapster;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>
/// Recalcul explicite d'un instantané financier déjà figé (cahier des charges §19.6, Lot 6 — voir
/// <see cref="ITimeEntryRevaluationService"/>) : action explicite (endpoint dédié), permission
/// dédiée (TIME_ENTRY_RECALCULATION, appliquée côté contrôleur), confirmation (l'appel HTTP
/// lui-même), motif obligatoire (vérifié ici), conservation de l'ancienne valeur dans l'audit
/// (§28.3 "recalcul financier explicite"). "Créer un nouveau snapshot" se traduit techniquement
/// par le remplacement des valeurs de l'instantané existant (relation 1-1 à clé partagée avec
/// TimeEntry, CLAUDE.md §11) : l'ancienne valeur n'est jamais perdue, elle est conservée
/// intégralement dans AuditLog.OldValue plutôt que dans une seconde ligne de snapshot. Aucun autre
/// enregistrement (TJM, contrat, autres saisies) n'est modifié : pas de recalcul rétroactif en
/// masse (§4.3).
/// </summary>
public class TimeEntryRevaluationService(
    IReadRepository<TimeEntry> timeEntryRepository,
    IRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    FinancialCalculationService financialCalculationService,
    AuditService auditService,
    ICurrentUser currentUser) : ITimeEntryRevaluationService
{
    public async Task<TimeEntryFinancialSnapshotDto> RecalculateAsync(
        Guid timeEntryId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessConflictException("Le motif est obligatoire pour un recalcul explicite (cahier des charges §19.6).");
        }

        var timeEntry = await timeEntryRepository.GetByIdAsync(timeEntryId, cancellationToken)
            ?? throw new BusinessConflictException("La saisie de temps indiquée n'existe pas.");

        // Clé partagée (Id == TimeEntryId) : GetByIdAsync renvoie l'instantané suivi par le
        // contexte, requis pour que la mutation ci-dessous soit persistée (CLAUDE.md §11).
        var snapshot = await snapshotRepository.GetByIdAsync(timeEntryId, cancellationToken)
            ?? throw new BusinessConflictException("Aucun instantané financier à recalculer pour cette saisie.");

        var oldValue = snapshot.Adapt<TimeEntryFinancialSnapshotDto>();

        var result = await financialCalculationService.CalculateAsync(
            new FinancialCalculationRequest { ResourceId = timeEntry.ResourceId, Date = timeEntry.Date, HeuresSaisies = timeEntry.DureeHeures },
            cancellationToken);

        var now = DateTime.UtcNow;
        FinancialCalculationService.ApplyToSnapshot(snapshot, result, now);
        snapshot.UpdatedAt = now;
        snapshot.UpdatedBy = currentUser.Identifier;

        var newValue = snapshot.Adapt<TimeEntryFinancialSnapshotDto>();

        await auditService.RecordAsync(AuditActions.Recalculation, nameof(TimeEntry), timeEntryId, oldValue, newValue, reason, cancellationToken);
        await snapshotRepository.SaveChangesAsync(cancellationToken);

        return newValue;
    }
}
