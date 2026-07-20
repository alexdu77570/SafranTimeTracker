using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Référentiel des types de projet (docs/BACKLOG_METIER.md §7, Lot 8) : axe de classification
/// (ex. Forfait / Régie / Interne / RUN) indépendant de <see cref="ProjectStatus"/>, qui reste
/// exclusivement le cycle de vie du projet (Actif/Suspendu/Terminé/Archivé).
/// </summary>
public class ProjectType : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
