using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Applications;

/// <summary>
/// Référentiel léger d'application (cahier des charges §15). Nommée "Application" au cahier
/// des charges ; renommée ApplicationReference pour éviter la collision avec le namespace de
/// couche SafranTimeTracker.Application (voir CLAUDE.md §6, docs/ARCHITECTURE.md §2).
///
/// Les champs statistiques (charge RUN/hors RUN, nb incidents/changes/problems/RITM) et les
/// projets associés sont des agrégats calculés à partir de données qui n'existent pas encore
/// (TimeEntry au Lot 3, Project au Lot 4) : ils ne sont donc pas modélisés ici.
/// </summary>
public class ApplicationReference : AuditableEntity
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public ApplicationCriticality Criticite { get; set; }
    public Guid? ResponsableId { get; set; }
    public Resource? Responsable { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }
}
