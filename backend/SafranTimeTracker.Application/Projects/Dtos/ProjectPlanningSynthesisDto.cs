namespace SafranTimeTracker.Application.Projects.Dtos;

/// <summary>
/// Synthèse planning/écarts d'un projet (cahier des charges §18.1, §29.5). "Reste à faire" n'est
/// pas ré-estimé indépendamment (pas de champ dédié à ce lot) : simplification documentée,
/// resteAFaire = chargeRestante, de sorte qu'atterrissageCharge = chargeAjustée (ou chargeInitiale
/// si aucune version ajustée). AtterrissageFinancier/RisqueBudget sont null sans
/// FINANCIAL_DATA_VIEW (CLAUDE.md §13) — seuls champs financiers de cette synthèse, le reste porte
/// sur la charge (heures), pas l'argent.
/// </summary>
public class ProjectPlanningSynthesisDto
{
    public Guid ProjectId { get; set; }

    public decimal ChargeInitiale { get; set; }
    public decimal? ChargeAjustee { get; set; }
    public decimal ChargeConsommee { get; set; }

    /// <summary>§29.5 : chargeAjustée (ou chargeInitiale si aucun ajustement) - chargeConsommée.</summary>
    public decimal ChargeRestante { get; set; }

    /// <summary>§29.5 : réalisé - prévu (prévu = chargeAjustée ou, à défaut, chargeInitiale).</summary>
    public decimal EcartCharge { get; set; }

    /// <summary>§29.5 : chargeAjustée - chargeInitiale (0 si aucune version ajustée).</summary>
    public decimal DeriveCharge { get; set; }

    /// <summary>§29.5 : chargeConsommée + resteÀFaire.</summary>
    public decimal AtterrissageCharge { get; set; }

    /// <summary>§29.5, en jours : (dateFinAjustée ou dateFinPrévueInitiale) - dateFinPrévueInitiale.</summary>
    public int DerivePlanningJours { get; set; }

    /// <summary>§29.5 : vrai si une date de fin ajustée dépasse la date de fin prévue initiale.</summary>
    public bool RisquePlanning { get; set; }

    /// <summary>§29.5/§14.3, formule MVP validée (Lot 5) : coûtRéelConsommé si aucune charge
    /// consommée, sinon extrapolation proportionnelle atterrissageCharge/chargeConsommée ×
    /// coûtRéelConsommé — pas de modèle prédictif avancé. Null sans FINANCIAL_DATA_VIEW.</summary>
    public decimal? AtterrissageFinancier { get; set; }

    /// <summary>§29.5 : vrai si AtterrissageFinancier dépasse le budget ajusté — recherché via le
    /// <c>Budget</c> lié au projet (Lot 5), avec repli documenté sur Project.BudgetInitial si aucun
    /// Budget n'est créé pour ce projet. Null sans FINANCIAL_DATA_VIEW ou si aucun budget
    /// (ajusté ou initial) n'est disponible.</summary>
    public bool? RisqueBudget { get; set; }
}
