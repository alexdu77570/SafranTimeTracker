using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Milestones;

/// <summary>
/// Référentiel des types de jalon (cahier des charges §24.1 : Kick-off, Architecture, VABE, VSR,
/// GO DEV, GO QUAL, GO VAL, GO PPROD, GO PROD, CAB, Hypercare — "les types sont administrables").
/// Créée en entité (comme <c>ActivityType</c>, Lot 3) malgré son absence de la liste minimum du
/// §30 : ce §30 liste un minimum, pas un maximum, et §24.1 exige explicitement l'administrabilité
/// — contrairement à <c>AbsenceType</c> (Lot 3, resté un enum, aucune mention d'administrabilité
/// au §23). Voir docs/IMPLEMENTATION_STATUS.md pour cette décision.
/// </summary>
public class MilestoneType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
