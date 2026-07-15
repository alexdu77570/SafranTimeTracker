namespace SafranTimeTracker.Application.Reporting.Dtos;

/// <summary>Résultat d'un export réel (cahier des charges §26.3) — jamais un bouton simulé.</summary>
public class ExportResultDto
{
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
