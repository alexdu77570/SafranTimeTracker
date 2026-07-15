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

    /// <summary>§29.5/§14.3, formule MVP validée (Lot 5) : reprend le facteur d'extrapolation de la
    /// charge (atterrissageCharge / chargeConsommée, cf. <see cref="CalculateChargeMetrics"/>) pour
    /// projeter le coût réel déjà consommé — pas de modèle prédictif indépendant. Sans charge
    /// consommée (chargeConsommee &lt;= 0), aucune extrapolation n'est possible : l'atterrissage
    /// vaut alors simplement le coût réel déjà consommé.</summary>
    public static decimal CalculateAtterrissageFinancier(decimal coutReelConsomme, decimal chargeConsommee, decimal atterrissageCharge) =>
        chargeConsommee <= 0 ? coutReelConsomme : atterrissageCharge / chargeConsommee * coutReelConsomme;

    /// <summary>§29.5 : "risque budget si l'atterrissage financier dépasse le budget ajusté" (formule
    /// littérale) — réutilisée telle quelle par <c>BudgetService</c> (§14.3) pour éviter de dupliquer
    /// la règle entre projet et budget générique.</summary>
    public static bool CalculateBudgetRisk(decimal atterrissageFinancier, decimal budgetAjuste) => atterrissageFinancier > budgetAjuste;
}
