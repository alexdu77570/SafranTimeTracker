using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Orders;

/// <summary>Ressources autorisées à utiliser une commande (cahier des charges §13.2).</summary>
public class OrderAuthorizedResource
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
}
