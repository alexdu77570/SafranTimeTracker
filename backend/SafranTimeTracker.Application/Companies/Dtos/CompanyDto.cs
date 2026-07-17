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

/// <summary>Code (clé métier) volontairement non modifiable ici, même convention que
/// Order.Reference (CLAUDE.md §5) — Lot 6, cahier des charges §28.3 "modification d'une
/// société".</summary>
public class CompanyUpdateRequest
{
    public string Nom { get; set; } = string.Empty;
    public Guid CompanyTypeId { get; set; }
    public string ContactPrincipal { get; set; } = string.Empty;
    public string EmailContact { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Commentaire { get; set; }
}
