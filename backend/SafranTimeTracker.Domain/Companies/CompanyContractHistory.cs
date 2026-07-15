using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Companies;

/// <summary>
/// Historique des contrats de prestation d'une société externe (cahier des charges §12.3). Le
/// contrat appartient à la société, jamais à une ressource. Données confidentielles : accès
/// gardé par la permission FINANCIAL_DATA_VIEW (CLAUDE.md §17), jamais par affichage seul.
/// </summary>
public class CompanyContractHistory : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string? ContractNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractDailyRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ReferentialStatus Status { get; set; } = ReferentialStatus.Actif;

    /// <summary>Jeton de concurrence optimiste (CLAUDE.md §11 : entité sensible contrat).</summary>
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
}
