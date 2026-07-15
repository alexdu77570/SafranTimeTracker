using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Resources;

/// <summary>
/// Variation de capacité d'une ressource sur une période (cahier des charges §10.5 : "la capacité
/// doit pouvoir varier selon une période", §30 — entité obligatoire). Mêmes règles d'intégrité que
/// les historiques financiers (docs/DATABASE.md §5 : une période ouverte, pas de chevauchement) —
/// sans jeton de concurrence : non listée parmi les entités sensibles TJM/contrats/budgets/
/// commandes (CLAUDE.md §11).
/// </summary>
public class ResourceCapacityPeriod : AuditableEntity
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }
    public string? Reason { get; set; }
    public ReferentialStatus Status { get; set; } = ReferentialStatus.Actif;
}
