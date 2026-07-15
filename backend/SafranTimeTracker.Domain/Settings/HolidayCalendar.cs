using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Settings;

/// <summary>Jour férié (cahier des charges §22.2, §29.2, §30 — entité obligatoire). Utilisé par
/// AvailabilityService pour le calcul de capacité réelle.</summary>
public class HolidayCalendar : AuditableEntity
{
    public DateOnly Date { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string Pays { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
