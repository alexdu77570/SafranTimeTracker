using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Organisation;

/// <summary>Cahier des charges §9.1.</summary>
public class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid? ResponsableId { get; set; }
    public Resource? Responsable { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
