namespace SafranTimeTracker.Application.Reporting.Dtos;

public class ChargesTopEntryDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal ChargeHeures { get; set; }
}

public class ChargesResourceAlertDto
{
    public Guid ResourceId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal ChargeHeures { get; set; }
    public decimal CapaciteReelle { get; set; }
}

/// <summary>§21.3 "courbe mensuelle" / §25.3 "évolution mensuelle" (Lot 12, décision 1). Un mois
/// n'apparaît que s'il porte au moins une saisie sur la période demandée.</summary>
public class ChargesMonthlyEvolutionDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public decimal ChargeTotaleHeures { get; set; }
    public decimal ChargeRunHeures { get; set; }
    public decimal ChargeHorsRunHeures { get; set; }
}

/// <summary>§21.3 "heatmap de charge" (`WorkloadHeatmap`, Lot 12, décision 1) : une ligne par
/// ressource × semaine ayant porté au moins une saisie, même granularité que `WeeklyPlanningGrid`
/// (Lot 10).</summary>
public class ChargesHeatmapEntryDto
{
    public Guid ResourceId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateOnly WeekStartDate { get; set; }
    public decimal ChargeHeures { get; set; }
}

/// <summary>§21.2/§21.3/§25.3 "prévu vs réalisé" à l'échelle du portefeuille filtré (Lot 12,
/// décision 3 — agrégation backend explicitement justifiée, docs/BACKLOG_METIER.md §16).</summary>
public class ChargesPlanComparisonDto
{
    /// <summary>Ajustée si une version Ajustée Active existe pour la semaine, sinon Initiale (même
    /// convention que ProjectPlanningCalculator, Lot 4). Null si le filtre porte sur une dimension
    /// que la planification ne connaît pas (commande, type d'activité) : non calculable à ce niveau
    /// de granularité, jamais approximé à zéro (CLAUDE.md §7).</summary>
    public decimal? ChargePrevue { get; set; }
    public decimal ChargeRealisee { get; set; }
}

/// <summary>
/// Indicateurs "Charges" (cahier des charges §21.2). Les décomptes par référence opérationnelle
/// (Incidents/Changes/Problems/RITM/VABE/VSR) comptent des références distinctes (TimeEntry.Reference),
/// pas des lignes de saisie — une même référence peut porter plusieurs saisies (§19.3), les compter
/// toutes fausserait le nombre réel d'incidents/changes traités. Surcharge/sous-charge (§29.6)
/// réutilisent AvailabilityService.GetAvailabilityAsync (Lot 3, capacité réelle), jamais recalculées
/// ici : les seuils (Settings.SeuilSurcharge/SeuilSousCharge) sont interprétés en pourcentage de la
/// capacité réelle sur la période.
/// </summary>
public class ChargesReportDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }

    public decimal ChargeTotaleHeures { get; set; }
    public decimal ChargeRunHeures { get; set; }
    public decimal ChargeHorsRunHeures { get; set; }

    public int NombreIncidents { get; set; }
    public int NombreChanges { get; set; }
    public int NombreProblems { get; set; }
    public int NombreRitm { get; set; }
    public int NombreVabe { get; set; }
    public int NombreVsr { get; set; }

    public IReadOnlyList<ChargesTopEntryDto> TopApplications { get; set; } = [];
    public IReadOnlyList<ChargesTopEntryDto> TopUtilisateurs { get; set; } = [];
    public IReadOnlyList<ChargesTopEntryDto> TopProjets { get; set; } = [];
    public IReadOnlyList<ChargesTopEntryDto> TopCommandes { get; set; } = [];

    public IReadOnlyList<ChargesResourceAlertDto> RessourcesSurchargees { get; set; } = [];
    public IReadOnlyList<ChargesResourceAlertDto> RessourcesSousChargees { get; set; } = [];

    public IReadOnlyList<ChargesMonthlyEvolutionDto> EvolutionMensuelle { get; set; } = [];
    public IReadOnlyList<ChargesHeatmapEntryDto> Heatmap { get; set; } = [];
    public ChargesPlanComparisonDto PrevuVsRealise { get; set; } = new();
}
