namespace SafranTimeTracker.Domain.Reporting;

/// <summary>
/// Journalisation d'un export réel (cahier des charges §26.3 "date et auteur de l'export",
/// "journalisation des exports financiers" ; §37.9). Entité légère et dédiée, volontairement
/// distincte de l'<c>AuditLog</c> général du §28.3 (Lot 6, toujours absent) : elle ne couvre que
/// les exports, pas l'ensemble des actions auditables du projet — ne pas anticiper l'audit
/// transversal du Lot 6. Append-only, jamais modifiée ni supprimée.
/// </summary>
public class ExportLog
{
    public Guid Id { get; set; }

    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;

    public string ReportType { get; set; } = string.Empty;
    public ExportFormat Format { get; set; }
    public string FiltersJson { get; set; } = string.Empty;
    public bool ContainsFinancialData { get; set; }
}
