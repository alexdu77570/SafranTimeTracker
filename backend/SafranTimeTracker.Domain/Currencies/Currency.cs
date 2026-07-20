using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Currencies;

/// <summary>
/// Référentiel des devises (docs/BACKLOG_METIER.md §9, Lot 8) : liste de consultation (code ISO
/// 4217, libellé, symbole), volontairement sans incidence sur FinancialCalculationService,
/// ResourceTjmHistory, CompanyContractHistory, Order ni Budget — tous les montants restent
/// implicitement en EUR. Un véritable support multi-devises est explicitement hors périmètre.
/// </summary>
public class Currency : AuditableEntity
{
    public string CodeIso { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Symbole { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
