using SafranTimeTracker.Application.TimeTracking.Dtos;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>
/// Recalcul explicite d'un instantané financier déjà figé (cahier des charges §19.6) : action
/// explicite, permission dédiée, confirmation, motif obligatoire, conservation de l'ancienne
/// valeur dans l'audit. Cette dernière exigence dépend d'AuditLog (Lot 6, non implémenté à ce
/// jour) : l'interface est posée dès le Lot 3 pour que TimeEntryService et les futurs
/// contrôleurs/permissions puissent en dépendre sans refonte d'architecture, mais aucun appelant
/// ne doit l'invoquer avant son implémentation réelle (voir <see cref="TimeEntryRevaluationService"/>).
/// </summary>
public interface ITimeEntryRevaluationService
{
    Task<TimeEntryFinancialSnapshotDto> RecalculateAsync(
        Guid timeEntryId, string reason, CancellationToken cancellationToken = default);
}
