using SafranTimeTracker.Application.TimeTracking.Dtos;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>
/// Espace réservé (Lot 6) : voir <see cref="ITimeEntryRevaluationService"/>. Volontairement non
/// implémenté tant qu'AuditLog n'existe pas — la conservation auditée de l'ancienne valeur est une
/// exigence non négociable du §19.6, pas une omission. Enregistré en DI (Application/DependencyInjection.cs)
/// pour que le câblage n'ait pas à changer au Lot 6, seul le corps de la méthode sera remplacé.
/// </summary>
public class TimeEntryRevaluationService : ITimeEntryRevaluationService
{
    public Task<TimeEntryFinancialSnapshotDto> RecalculateAsync(
        Guid timeEntryId, string reason, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException(
            "Recalcul explicite différé au Lot 6 (dépend d'AuditLog, cahier des charges §19.6).");
}
