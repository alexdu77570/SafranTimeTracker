using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Dtos;

/// <summary>Filtres communs aux rapports (cahier des charges §21.1). Tous facultatifs sauf la
/// période (§26.3 "respect des filtres").</summary>
public class ReportingFilterQuery
{
    public ReportingPeriodType PeriodType { get; set; } = ReportingPeriodType.Mois;
    public DateOnly ReferenceDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? CustomFrom { get; set; }
    public DateOnly? CustomTo { get; set; }

    public Guid? ApplicationId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid? ActivityTypeId { get; set; }
    public Guid? OperationalRoleId { get; set; }
}
