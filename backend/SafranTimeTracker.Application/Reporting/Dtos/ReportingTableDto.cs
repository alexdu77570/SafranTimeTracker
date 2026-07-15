namespace SafranTimeTracker.Application.Reporting.Dtos;

/// <summary>
/// Représentation tabulaire générique d'un rapport, produite par ReportingService (jamais par
/// ExportService, qui ne fait que restituer — CLAUDE.md §10 : aucun calcul métier hors des services
/// nommés). Permet à ExportService de générer CSV/Excel/PDF avec un seul rendu par format, quel que
/// soit le rapport source (cahier des charges §26.3 : exports cohérents avec l'écran, §37.9).
/// </summary>
public class ReportingTableDto
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<string> Columns { get; set; } = [];
    public IReadOnlyList<string[]> Rows { get; set; } = [];
}
