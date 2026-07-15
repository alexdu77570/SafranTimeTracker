using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Organisation;

/// <summary>
/// Cahier des charges §9.3 (équipe). Le responsable fonctionnel n'est pas un rôle applicatif
/// distinct : c'est un rattachement organisationnel facultatif (§9.3).
/// </summary>
public class Team : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid? ResponsableFonctionnelId { get; set; }
    public Resource? ResponsableFonctionnel { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }
}
