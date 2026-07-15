namespace SafranTimeTracker.Application.Orders.Dtos;

/// <summary>
/// Ne porte que les champs administratifs du Lot 1 (cahier des charges §13.2). Budget ajusté,
/// consommation, coûts, différentiel et restes sont différés (Lots 2/3/5) — voir docs/IMPLEMENTATION_STATUS.md.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public decimal BudgetFinancierInitial { get; set; }
    public decimal? BudgetJoursInitial { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }
    public Guid StatusId { get; set; }
    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> AuthorizedResourceIds { get; set; } = [];
}

public class OrderCreateRequest
{
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public decimal BudgetFinancierInitial { get; set; }
    public decimal? BudgetJoursInitial { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }
    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> AuthorizedResourceIds { get; set; } = [];
}
