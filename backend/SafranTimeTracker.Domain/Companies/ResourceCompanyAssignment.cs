using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Companies;

/// <summary>
/// Rattachement historisé d'une ressource à une société (cahier des charges §12.2). Source de
/// vérité utilisée par FinancialCalculationService pour déterminer la société applicable à la
/// date d'une saisie. Distinct de <see cref="Resource.CompanyId"/> (Lot 1), qui reste un pointeur
/// d'affichage "société courante" non historisé — voir docs/IMPLEMENTATION_STATUS.md.
/// </summary>
public class ResourceCompanyAssignment : AuditableEntity
{
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; } = ReferentialStatus.Actif;
}
