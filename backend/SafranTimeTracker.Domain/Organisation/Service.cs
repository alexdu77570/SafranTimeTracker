using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Organisation;

/// <summary>Cahier des charges §9.2.</summary>
public class Service : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid? ResponsableId { get; set; }
    public Resource? Responsable { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }

    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
