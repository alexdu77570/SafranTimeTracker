using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Projects.Dtos;

public class ProjectParticipantDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid? OperationalRoleId { get; set; }
    public Guid? DefaultOrderId { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public decimal? CapacitePrevue { get; set; }
    public ReferentialStatus Statut { get; set; }

    /// <summary>Société applicable, TJM et coûts (§17.2) — null sans FINANCIAL_DATA_VIEW, calculés
    /// par ProjectParticipantService (jamais stockés, CLAUDE.md §13).</summary>
    public ProjectParticipantFinancialSummaryDto? FinancialSummary { get; set; }
}

/// <summary>Cahier des charges §17.2. Société applicable/TJM/coûts calculés à la date du jour pour
/// l'affichage de la fiche (pas une date de saisie : il n'y a pas de saisie unique pour un
/// participant, contrairement à TimeEntry).</summary>
public class ProjectParticipantFinancialSummaryDto
{
    public Guid? CompanyIdApplicable { get; set; }
    public decimal? TjmPersonneApplicable { get; set; }
    public decimal? TjmContratApplicable { get; set; }
}

public class ProjectParticipantCreateRequest
{
    public Guid ResourceId { get; set; }
    public Guid? OperationalRoleId { get; set; }
    public Guid? DefaultOrderId { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public decimal? CapacitePrevue { get; set; }
}
