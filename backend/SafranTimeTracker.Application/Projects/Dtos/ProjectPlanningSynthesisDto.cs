namespace SafranTimeTracker.Application.Projects.Dtos;

/// <summary>
/// Synthèse planning/écarts d'un projet (cahier des charges §18.1, §29.5). "Reste à faire" n'est
/// pas ré-estimé indépendamment (pas de champ dédié à ce lot) : simplification documentée,
/// resteAFaire = chargeRestante, de sorte qu'atterrissageCharge = chargeAjustée (ou chargeInitiale
/// si aucune version ajustée). RisqueBudget est null sans FINANCIAL_DATA_VIEW (CLAUDE.md §13) —
/// seul champ financier de cette synthèse, le reste porte sur la charge (heures), pas l'argent.
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

    /// <summary>§29.5 : vrai si le coût réel déjà consommé dépasse le budget initial — simplification
    /// documentée (pas de "budget ajusté"/rallonge avant le Lot 5, donc pas d'atterrissage financier
    /// au sens strict du cahier). Null sans FINANCIAL_DATA_VIEW.</summary>
    public bool? RisqueBudget { get; set; }
}
