namespace SafranTimeTracker.Domain.Reporting;

/// <summary>Formats d'export réel attendus (cahier des charges §26.3) : CSV natif, Excel
/// (ClosedXML), PDF (QuestPDF) — jamais de bouton simulé (CLAUDE.md §42, régressions interdites).</summary>
public enum ExportFormat
{
    Csv,
    Excel,
    Pdf
}
