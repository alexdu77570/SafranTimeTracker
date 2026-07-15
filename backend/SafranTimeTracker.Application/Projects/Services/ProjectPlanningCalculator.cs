namespace SafranTimeTracker.Application.Projects.Services;

/// <summary>
/// Formules d'écart et de risque projet (cahier des charges §29.5), extraites en fonctions pures
/// pour rester testables sans base de données (CLAUDE.md §14 : "écarts projet"). Seul
/// ProjectPlanningService les invoque — aucune règle dupliquée ailleurs.
/// </summary>
public static class ProjectPlanningCalculator
{
    public record ChargeMetrics(decimal ChargeRestante, decimal EcartCharge, decimal DeriveCharge, decimal AtterrissageCharge);

    public record PlanningRisk(int DerivePlanningJours, bool RisquePlanning);

    /// <summary>"Prévu" = chargeAjustée, ou à défaut chargeInitiale (§29.5). "Reste à faire" =
    /// chargeRestante (simplification documentée : pas de ré-estimation indépendante à ce lot).</summary>
    public static ChargeMetrics CalculateChargeMetrics(decimal chargeInitiale, decimal? chargeAjustee, decimal chargeConsommee)
    {
        var chargePrevueReference = chargeAjustee ?? chargeInitiale;
        var chargeRestante = chargePrevueReference - chargeConsommee;
        var ecartCharge = chargeConsommee - chargePrevueReference;
        var deriveCharge = chargePrevueReference - chargeInitiale;
        var atterrissageCharge = chargeConsommee + chargeRestante;

        return new ChargeMetrics(chargeRestante, ecartCharge, deriveCharge, atterrissageCharge);
    }

    public static PlanningRisk CalculatePlanningRisk(DateOnly dateFinPrevueInitiale, DateOnly? dateFinAjustee)
    {
        var dateFinReference = dateFinAjustee ?? dateFinPrevueInitiale;
        var derivePlanningJours = dateFinReference.DayNumber - dateFinPrevueInitiale.DayNumber;
        var risquePlanning = dateFinAjustee is not null && dateFinAjustee > dateFinPrevueInitiale;

        return new PlanningRisk(derivePlanningJours, risquePlanning);
    }

    /// <summary>Simplification documentée (Lot 4) : pas de "budget ajusté"/rallonge avant le Lot 5,
    /// donc pas d'atterrissage financier au sens strict du cahier — le risque se lit ici sur le
    /// coût réel déjà consommé face au budget initial.</summary>
    public static bool CalculateBudgetRisk(decimal coutReelConsomme, decimal budgetInitial) => coutReelConsomme > budgetInitial;
}
