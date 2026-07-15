using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Companies.Dtos;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CompanyTypeId { get; set; }
    public ReferentialStatus Statut { get; set; }
    public string ContactPrincipal { get; set; } = string.Empty;
    public string EmailContact { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Commentaire { get; set; }
}

public class CompanyCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CompanyTypeId { get; set; }
    public string ContactPrincipal { get; set; } = string.Empty;
    public string EmailContact { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Commentaire { get; set; }
}
