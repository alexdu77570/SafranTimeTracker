using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Companies;

/// <summary>
/// Société interne ou externe (cahier des charges §12.1). L'historique des contrats
/// (CompanyContractHistory) et les rattachements ressource/société historisés
/// (ResourceCompanyAssignment) sont différés au Lot 2 (docs/DATABASE.md §5).
/// </summary>
public class Company : AuditableEntity
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CompanyTypeId { get; set; }
    public CompanyType CompanyType { get; set; } = null!;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string ContactPrincipal { get; set; } = string.Empty;
    public string EmailContact { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Commentaire { get; set; }
}
