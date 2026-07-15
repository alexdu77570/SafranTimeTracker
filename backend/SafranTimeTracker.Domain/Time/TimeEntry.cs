using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Time;

/// <summary>
/// Saisie de temps (cahier des charges §19.1). Le lien vers Project est explicitement hors
/// périmètre du Lot 3 (Project n'existe pas encore, Lot 4) — même logique que
/// Resource.DefaultOrderId au Lot 1. Le champ "semaine" du §19.1 n'est pas stocké : il se calcule
/// à la volée depuis Date pour le filtre (§19.4), pour éviter une donnée dérivée qui peut devenir
/// incohérente. "Utilisateur" (§19.1) n'est pas un champ distinct : CreatedBy/UpdatedBy
/// (AuditableEntity) couvrent l'auteur réel de la saisie/correction, ResourceId porte la personne
/// concernée.
/// </summary>
public class TimeEntry : AuditableEntity
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public Guid ActivityTypeId { get; set; }
    public ActivityType ActivityType { get; set; } = null!;

    /// <summary>Commande facultative (§19.1). Doit être compatible avec la société de la
    /// ressource à la date de la saisie (§13.4), vérifié par TimeEntryService.</summary>
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public DateOnly Date { get; set; }
    public decimal DureeHeures { get; set; }
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;

    public TimeEntryFinancialSnapshot? FinancialSnapshot { get; set; }
}
