namespace SafranTimeTracker.Application.Reporting.Dtos;

public class OperationalReportGroupDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal ChargeHeures { get; set; }
}

public class OperationalReportMilestoneDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public DateOnly DatePrevue { get; set; }
}

public class OperationalReportCapacityDto
{
    public Guid ResourceId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal CapaciteTheorique { get; set; }
    public decimal CapaciteReelle { get; set; }
    public decimal TauxDisponibilite { get; set; }
}

/// <summary>Cahier des charges §26.1. Aucune donnée financière — accessible sans permission
/// dédiée, contrairement à FinancialReportDto.</summary>
public class OperationalReportDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }

    public IReadOnlyList<OperationalReportGroupDto> ChargeParEquipe { get; set; } = [];
    public IReadOnlyList<OperationalReportGroupDto> ChargeParService { get; set; } = [];
    public IReadOnlyList<OperationalReportGroupDto> ChargeParDepartement { get; set; } = [];
    public IReadOnlyList<ChargesTopEntryDto> ConsommationParProjet { get; set; } = [];
    public IReadOnlyList<ChargesTopEntryDto> ConsommationParCommande { get; set; } = [];
    public IReadOnlyList<OperationalReportMilestoneDto> JalonsEnRetard { get; set; } = [];
    public IReadOnlyList<ChargesResourceAlertDto> RessourcesSurchargees { get; set; } = [];
    public IReadOnlyList<ChargesResourceAlertDto> RessourcesSousUtilisees { get; set; } = [];
    public IReadOnlyList<OperationalReportCapacityDto> CapaciteEtDisponibilite { get; set; } = [];
}
