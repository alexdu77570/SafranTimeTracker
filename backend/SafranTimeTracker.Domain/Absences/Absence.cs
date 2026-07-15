using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Absences;

/// <summary>
/// Demande d'absence (cahier des charges §23.1). Seule une absence au statut Validé réduit la
/// capacité réelle, sauf paramètre contraire (§23.3, Settings.ActivationValidationAbsences) —
/// voir AvailabilityService.
/// </summary>
public class Absence : AuditableEntity
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public AbsenceType Type { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public bool DemiJournee { get; set; }
    public string? Commentaire { get; set; }
    public AbsenceStatus Statut { get; set; } = AbsenceStatus.Brouillon;

    public string? ValideParIdentifiant { get; set; }
    public DateTime? DateDecision { get; set; }
}
