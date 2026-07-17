using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Dtos;

/// <summary>§27.3 étapes 1-4 : type/mode/fichier/encodage-séparateur, avant tout mapping. Aucune
/// validation ni persistance.</summary>
public class ImportPreviewDto
{
    public IReadOnlyList<string> DetectedHeaders { get; set; } = [];
    public IReadOnlyList<string> ExpectedHeaders { get; set; } = [];
    public int LineCount { get; set; }
    public IReadOnlyList<IReadOnlyDictionary<string, string>> SampleRows { get; set; } = [];
}

public class FieldChangeDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ImportRowResultDto
{
    public int RowNumber { get; set; }
    public Guid? EntityId { get; set; }
    public ImportDiffType DiffType { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<FieldChangeDto> Changes { get; set; } = [];
}

/// <summary>§27.3 étapes 5-9 : validation, erreurs, simulation — aucune persistance (§27.4 "ne pas
/// modifier les données avant confirmation").</summary>
public class ImportSimulationDto
{
    public int LineCount { get; set; }
    public int AddCount { get; set; }
    public int UpdateCount { get; set; }
    public int UnchangedCount { get; set; }
    public int DeleteCount { get; set; }
    public int ErrorCount { get; set; }
    public IReadOnlyList<ImportRowResultDto> Rows { get; set; } = [];
}

public class ImportBatchDto
{
    public Guid Id { get; set; }
    public ImportEntityType Type { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ImportMode Mode { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public int AddCount { get; set; }
    public int UpdateCount { get; set; }
    public int DeleteCount { get; set; }
    public int ErrorCount { get; set; }
    public ImportBatchStatus Status { get; set; }
    public string? Errors { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public Guid? PreviousBatchId { get; set; }
}

public class ImportDiffDto
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public ImportDiffType DiffType { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ImportTypeMetadataDto
{
    public ImportEntityType Type { get; set; }
    public IReadOnlyList<string> ExpectedHeaders { get; set; } = [];
    public IReadOnlyList<ImportMode> SupportedModes { get; set; } = [];
}
