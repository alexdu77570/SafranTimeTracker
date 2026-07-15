namespace SafranTimeTracker.Domain.Absences;

/// <summary>Cycle de vie d'une demande d'absence (cahier des charges §23.1). Enum C#, absent de la
/// liste d'entités minimum du §30 (contrairement à OrderStatus/ProjectStatus qui y figurent).</summary>
public enum AbsenceStatus
{
    Brouillon,
    Soumis,
    Valide,
    Refuse,
    Annule
}
