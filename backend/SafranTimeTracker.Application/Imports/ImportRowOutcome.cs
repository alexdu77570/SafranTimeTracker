using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports;

/// <summary>Résultat du traitement d'une ligne, que l'appel ait persisté ou non (§27.4 : aperçu et
/// simulation partagent exactement la même logique de validation/diff que l'exécution — seule la
/// persistance change).</summary>
public class ImportRowOutcome
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? EntityId { get; init; }
    public ImportDiffType DiffType { get; init; }
    public IReadOnlyList<FieldChange> Changes { get; init; } = [];

    public static ImportRowOutcome Error(string message) =>
        new() { Success = false, ErrorMessage = message, DiffType = ImportDiffType.Erreur };
}
