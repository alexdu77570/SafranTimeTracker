namespace SafranTimeTracker.Domain.Orders;

/// <summary>Brouillon, Active, Suspendue, Consommée, Clôturée (cahier des charges §13.2, §30).</summary>
public class OrderStatus
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public int Ordre { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
