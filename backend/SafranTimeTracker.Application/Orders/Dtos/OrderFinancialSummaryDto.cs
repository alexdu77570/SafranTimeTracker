namespace SafranTimeTracker.Application.Orders.Dtos;

/// <summary>
/// Sous-objet financier isolé (cahier des charges §13.2 : "consommation en jours", "coût réel
/// consommé", "coût contractuel consommé", "différentiel", "reste financier", "reste en jours").
/// Consommation agrégée depuis TimeEntry/TimeEntryFinancialSnapshot liés via TimeEntry.OrderId
/// (§20.6), comparée au budget <b>ajusté</b> (référence après rallonges éventuelles, §13.3) —
/// jamais au budget initial, qui reste un simple repère d'origine.
/// </summary>
public class OrderFinancialSummaryDto
{
    public decimal ConsommationJours { get; set; }
    public decimal CoutReelConsomme { get; set; }
    public decimal CoutContractuelConsomme { get; set; }
    public decimal Differentiel { get; set; }
    public decimal RestFinancier { get; set; }

    /// <summary>Null si aucun budget en jours ajusté n'est renseigné.</summary>
    public decimal? RestJours { get; set; }
}
