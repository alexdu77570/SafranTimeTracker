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
}
