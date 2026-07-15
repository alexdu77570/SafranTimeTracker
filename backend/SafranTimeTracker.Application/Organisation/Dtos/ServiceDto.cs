using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Organisation.Dtos;

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid? ResponsableId { get; set; }
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
}

public class ServiceCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid? ResponsableId { get; set; }
    public string? Commentaire { get; set; }
}
