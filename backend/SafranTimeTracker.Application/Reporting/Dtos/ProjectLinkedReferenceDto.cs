namespace SafranTimeTracker.Application.Reporting.Dtos;

/// <summary>
/// Cahier des charges §17.7 (onglet "Références liées") : références INC/CHG/PRB/RITM/VABE/VSR
/// portées par les saisies de temps d'un projet. Ferme l'écart identifié à la clôture du Lot 4
/// (aucun endpoint de synthèse dédié n'existait alors) — dérivé de TimeEntry.Reference/ActivityType,
/// sans nouvelle entité de stockage (docs/IMPLEMENTATION_STATUS.md).
/// </summary>
public class ProjectLinkedReferenceDto
{
    public string Reference { get; set; } = string.Empty;
    public string ActivityTypeCode { get; set; } = string.Empty;
    public string ActivityTypeLibelle { get; set; } = string.Empty;
    public int NombreSaisies { get; set; }
    public decimal ChargeHeures { get; set; }
    public DateOnly PremiereDate { get; set; }
    public DateOnly DerniereDate { get; set; }
}
