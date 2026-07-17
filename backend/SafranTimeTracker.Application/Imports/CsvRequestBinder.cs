using System.Globalization;
using System.Reflection;

namespace SafranTimeTracker.Application.Imports;

/// <summary>
/// Lie une ligne CSV (dictionnaire en-tête → valeur) aux propriétés publiques d'un DTO de requête
/// existant (XxxCreateRequest/XxxUpdateRequest, Lots 1 à 5) par correspondance de nom — même
/// principe que Mapster (déjà accepté par CLAUDE.md §13 comme mapping par réflexion), généralisé
/// une seule fois plutôt que dupliqué par type importable (cahier des charges §27.1, Lot 6).
/// MVP sans assistant de correspondance interactif (aucun écran frontend, §27.3 étape 5 différée) :
/// les en-têtes attendus correspondent exactement aux noms de propriété (voir
/// <c>IImportAdapter.ExpectedHeaders</c>). Une colonne absente ou vide laisse la propriété à sa
/// valeur par défaut.
/// </summary>
public static class CsvRequestBinder
{
    public static TRequest Bind<TRequest>(IReadOnlyDictionary<string, string> row) where TRequest : new()
    {
        var request = new TRequest();
        foreach (var property in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || !row.TryGetValue(property.Name, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
            {
                continue;
            }

            property.SetValue(request, ConvertValue(rawValue.Trim(), property.PropertyType));
        }

        return request;
    }

    /// <summary>Identifiant technique de la ligne (§27.3 : présent = mise à jour de cette entité,
    /// absent = ajout). Jamais un champ des DTO Create/UpdateRequest eux-mêmes (générés
    /// côté serveur à la création).</summary>
    public static Guid? ReadOptionalId(IReadOnlyDictionary<string, string> row) =>
        row.TryGetValue("Id", out var raw) && !string.IsNullOrWhiteSpace(raw) ? Guid.Parse(raw.Trim()) : null;

    public static string? ReadOptional(IReadOnlyDictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var raw) && !string.IsNullOrWhiteSpace(raw) ? raw.Trim() : null;

    public static Guid ReadRequiredGuid(IReadOnlyDictionary<string, string> row, string key) =>
        Guid.Parse(RequireValue(row, key));

    public static DateOnly ReadRequiredDate(IReadOnlyDictionary<string, string> row, string key) =>
        DateOnly.Parse(RequireValue(row, key), CultureInfo.InvariantCulture);

    public static decimal ReadRequiredDecimal(IReadOnlyDictionary<string, string> row, string key) =>
        decimal.Parse(RequireValue(row, key), CultureInfo.InvariantCulture);

    private static string RequireValue(IReadOnlyDictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var raw) && !string.IsNullOrWhiteSpace(raw)
            ? raw.Trim()
            : throw new FormatException($"La colonne '{key}' est obligatoire et absente de la ligne.");

    private static object ConvertValue(string rawValue, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(Guid))
        {
            return Guid.Parse(rawValue);
        }
        if (underlyingType == typeof(DateOnly))
        {
            return DateOnly.Parse(rawValue, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(DateTime))
        {
            return DateTime.Parse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }
        if (underlyingType == typeof(decimal))
        {
            return decimal.Parse(rawValue, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(bool))
        {
            return bool.Parse(rawValue);
        }
        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, rawValue, ignoreCase: true);
        }
        if (underlyingType == typeof(string))
        {
            return rawValue;
        }
        if (typeof(IEnumerable<Guid>).IsAssignableFrom(underlyingType))
        {
            return rawValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToList();
        }

        throw new NotSupportedException($"Type de colonne non supporté par l'import : {targetType.Name}.");
    }
}
