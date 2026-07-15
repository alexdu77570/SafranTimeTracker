using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Resources.Dtos;

public class ResourceDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ResponsableHierarchiqueId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? DefaultOrderId { get; set; }
    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> OperationalRoleIds { get; set; } = [];
}

public class ResourceCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ResponsableHierarchiqueId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? DefaultOrderId { get; set; }
    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }
    public string? Commentaire { get; set; }
    public IReadOnlyList<Guid> OperationalRoleIds { get; set; } = [];
}
