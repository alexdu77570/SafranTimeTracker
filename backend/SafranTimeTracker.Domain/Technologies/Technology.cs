using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Technologies;

/// <summary>
/// Référentiel des technologies (docs/BACKLOG_METIER.md §5, Lot 8). Rattachable aux Applications
/// (stack technique, visible dans le détail statistique de l'application) et aux Ressources
/// (compétences maîtrisées), deux relations many-to-many indépendantes.
/// </summary>
public class Technology : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;

    public ICollection<ApplicationTechnology> Applications { get; set; } = new List<ApplicationTechnology>();
    public ICollection<ResourceTechnology> Resources { get; set; } = new List<ResourceTechnology>();
}
