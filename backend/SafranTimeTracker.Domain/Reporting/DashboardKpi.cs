using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Reporting;

/// <summary>
/// Référentiel administrable des KPI disponibles au tableau de bord (cahier des charges §30,
/// catégorie "Paramétrage" ; §25.1/§25.2 pour la liste des KPI attendus). Même principe que
/// <c>MilestoneType</c>/<c>ActivityType</c> : seule la définition du KPI (code, libellé,
/// catégorie, ordre, actif/inactif) est une donnée persistée — la <b>valeur</b> de chaque KPI
/// n'est jamais stockée, toujours calculée à la demande par <c>ReportingService</c> (CLAUDE.md §7,
/// absence de recalcul rétroactif : aucun agrégat figé qui pourrait diverger de la réalité).
/// </summary>
public class DashboardKpi : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public DashboardKpiCategory Category { get; set; }
    public int Ordre { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
}
