using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Applications.Dtos;

/// <summary>
/// Référentiel léger d'application (cahier des charges §15). Les statistiques (charge RUN/hors
/// RUN, incidents, changes, problems, RITM) et les projets associés sont différés (Lots 3/4) —
/// voir docs/IMPLEMENTATION_STATUS.md.
/// </summary>
public class ApplicationReferenceDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public ApplicationCriticality Criticite { get; set; }
    public Guid? ResponsableId { get; set; }
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
}

public class ApplicationReferenceCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public ApplicationCriticality Criticite { get; set; }
    public Guid? ResponsableId { get; set; }
    public string? Commentaire { get; set; }
}
