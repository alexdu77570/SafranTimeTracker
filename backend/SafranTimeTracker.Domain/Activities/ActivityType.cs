using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Activities;

/// <summary>
/// Référentiel des types d'activité de saisie de temps (cahier des charges §19.2, §30 — entité
/// obligatoire, contrairement à AbsenceType qui n'y figure pas). Porte à la fois la classification
/// RUN/hors RUN (§29.4) et les métadonnées de validation de référence (§19.3), pour que la règle
/// « la validation du format dépend du type d'activité » (§19.3) reste entièrement pilotée par la
/// donnée : aucun type codé en dur dans un validateur.
/// </summary>
public class ActivityType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Classification RUN/hors RUN (§29.4). "Administrable à terme" (§29.4) : porté par
    /// la donnée, pas par une liste codée en dur.</summary>
    public bool IsRun { get; set; }

    public bool ReferenceRequired { get; set; }
    public string? ReferenceFormatRegex { get; set; }
    public string? ReferenceExample { get; set; }

    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
