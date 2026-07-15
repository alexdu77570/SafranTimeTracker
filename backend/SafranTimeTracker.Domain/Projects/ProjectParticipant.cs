using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Participation d'une ressource à un projet (cahier des charges §17.2). Société applicable, TJM
/// et coûts ne sont pas stockés : calculés à la demande via
/// <c>FinancialCalculationService.GetApplicableCompanyIdAsync</c> (Lot 2) et
/// <c>ResourceTjmHistory</c>, jamais dupliqués.
/// </summary>
public class ProjectParticipant : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public Guid? OperationalRoleId { get; set; }
    public OperationalRole? OperationalRole { get; set; }

    /// <summary>Commande par défaut sur CE projet (§17.2) — distincte de
    /// <see cref="Resource.DefaultOrderId"/> (Lot 1, portée globale à la ressource).</summary>
    public Guid? DefaultOrderId { get; set; }
    public Order? DefaultOrder { get; set; }

    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public decimal? CapacitePrevue { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
