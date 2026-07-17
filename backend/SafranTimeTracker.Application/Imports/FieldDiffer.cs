using System.Reflection;

namespace SafranTimeTracker.Application.Imports;

public sealed record FieldChange(string FieldName, string? OldValue, string? NewValue);

/// <summary>
/// Calcule les champs modifiés entre l'état existant d'une entité (projeté en DTO de lecture) et
/// une requête de mise à jour, pour alimenter <c>ImportDiff</c> (§27.6) — une seule implémentation
/// par réflexion plutôt qu'une comparaison écrite à la main pour chacun des 16 types importables.
/// Compare uniquement les propriétés portées par <typeparamref name="TRequest"/> (les seules que
/// l'import modifie réellement).
/// </summary>
public static class FieldDiffer
{
    public static IReadOnlyList<FieldChange> Diff<TReadDto, TRequest>(TReadDto oldValue, TRequest newValue)
    {
        var changes = new List<FieldChange>();
        var oldType = typeof(TReadDto);

        foreach (var requestProperty in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var oldProperty = oldType.GetProperty(requestProperty.Name, BindingFlags.Public | BindingFlags.Instance);
            if (oldProperty is null)
            {
                continue;
            }

            var oldRaw = oldProperty.GetValue(oldValue);
            var newRaw = requestProperty.GetValue(newValue);
            var oldText = FormatValue(oldRaw);
            var newText = FormatValue(newRaw);

            if (oldText != newText)
            {
                changes.Add(new FieldChange(requestProperty.Name, oldText, newText));
            }
        }

        return changes;
    }

    private static string? FormatValue(object? value) => value switch
    {
        null => null,
        IEnumerable<Guid> guids => string.Join(';', guids),
        _ => value.ToString()
    };
}
