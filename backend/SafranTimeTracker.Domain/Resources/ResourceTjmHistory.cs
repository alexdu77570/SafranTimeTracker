using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Resources;

/// <summary>
/// Historique du TJM d'une ressource (cahier des charges §11). Le TJM appartient à la personne :
/// il ne dépend jamais du projet, de la commande, du type interne/externe ni du rôle opérationnel
/// (§11.1). La valeur applicable à une date se recherche dans cet historique à la date de la
/// saisie, jamais à la date courante (§11.3) — voir FinancialCalculationService.
/// </summary>
public class ResourceTjmHistory : AuditableEntity
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyRate { get; set; }
    public string? Reason { get; set; }
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; } = ReferentialStatus.Actif;

    /// <summary>Jeton de concurrence optimiste (CLAUDE.md §11 : entité sensible TJM).</summary>
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
}
