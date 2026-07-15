using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Dtos;

public class DashboardKpiDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public DashboardKpiCategory Category { get; set; }
    public int Ordre { get; set; }
    public ReferentialStatus Statut { get; set; }
}

public class DashboardKpiCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public DashboardKpiCategory Category { get; set; }
    public int Ordre { get; set; }
}
