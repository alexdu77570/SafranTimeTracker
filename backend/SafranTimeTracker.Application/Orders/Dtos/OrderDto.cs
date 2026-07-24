namespace SafranTimeTracker.Application.Orders.Dtos;

/// <summary>
/// Budget initial, jours initiaux et date de fin initiale sont figés à la création (cahier des
/// charges §13.2 : engagement contractuel d'origine) — seules les rallonges (<see cref="OrderExtension"/>,
/// §13.3) font évoluer le budget/jours/date "ajusté". Le sous-objet financier est omis sans
/// FINANCIAL_DATA_VIEW (CLAUDE.md §13), même principe que <c>ProjectDto.FinancialSummary</c>.
///
/// <para>Sous-lot 14.3 (rapport d'audit du Lot 14, constat SEC-3) : <see cref="BudgetFinancierInitial"/>/
/// <see cref="BudgetFinancierAjuste"/> sont désormais nullables et omis sans FINANCIAL_DATA_VIEW au
/// même titre que <see cref="BudgetJoursInitial"/>/<see cref="BudgetJoursAjuste"/>/<see cref="SeuilAlerte"/>
/// — avant ce correctif, ces deux champs restaient toujours présents en racine du DTO (fuite
/// financière complète), contrairement au sous-objet <see cref="FinancialSummary"/> déjà filtré.</para>
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal? BudgetFinancierInitial { get; set; }
    public decimal? BudgetFinancierAjuste { get; set; }
    public decimal? BudgetJoursInitial { get; set; }
    public decimal? BudgetJoursAjuste { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }
    public DateOnly? DateFinAjustee { get; set; }
    public Guid StatusId { get; set; }
    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> AuthorizedResourceIds { get; set; } = [];

    /// <summary>Null sans FINANCIAL_DATA_VIEW (projection faite par OrderService, CLAUDE.md §13).</summary>
    public OrderFinancialSummaryDto? FinancialSummary { get; set; }
}

public class OrderCreateRequest
{
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal BudgetFinancierInitial { get; set; }
    public decimal? BudgetJoursInitial { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinInitiale { get; set; }
    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> AuthorizedResourceIds { get; set; } = [];
}

/// <summary>Budget/jours initiaux, date de fin initiale et statut ne sont pas modifiables ici :
/// les rallonges (§13.3) et les actions de transition de statut dédiées en tiennent lieu.</summary>
public class OrderUpdateRequest
{
    public string Libelle { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public decimal? SeuilAlerte { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> AuthorizedResourceIds { get; set; } = [];
}

/// <summary>Réouverture d'une commande clôturée (§13.4) : action exceptionnelle, motif obligatoire
/// (même pattern que ProjectPlanVersionAdjustmentRequest, Lot 4).</summary>
public class OrderReopenRequest
{
    public string Motif { get; set; } = string.Empty;
}
