using System.Text;

namespace SafranTimeTracker.Application.Imports;

public sealed record ParsedCsv(IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyDictionary<string, string>> Rows);

/// <summary>
/// Analyse d'un fichier CSV (cahier des charges §27.3 étape 4 : "détection de l'encodage et du
/// séparateur"). Encodage : UTF-8, avec détection du BOM éventuel. Séparateur : virgule ou
/// point-virgule, déterminé par comptage sur la ligne d'en-tête (le point-virgule est plus fréquent
/// dans les exports français/Excel). Fonction pure, testée sans base de données (CLAUDE.md §14).
/// </summary>
public static class CsvFileParser
{
    public static ParsedCsv Parse(byte[] content)
    {
        using var reader = new StreamReader(new MemoryStream(content), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        var lines = SplitLines(text);
        if (lines.Count == 0 || string.IsNullOrWhiteSpace(lines[0]))
        {
            return new ParsedCsv([], []);
        }

        var separator = DetectSeparator(lines[0]);
        var headers = SplitLine(lines[0], separator);

        var rows = new List<IReadOnlyDictionary<string, string>>();
        for (var i = 1; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            var values = SplitLine(lines[i], separator);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                dict[headers[c]] = c < values.Count ? values[c] : string.Empty;
            }

            rows.Add(dict);
        }

        return new ParsedCsv(headers, rows);
    }

    public static string ComputeChecksum(byte[] content) =>
        Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(content));

    private static char DetectSeparator(string headerLine) =>
        headerLine.Count(c => c == ';') > headerLine.Count(c => c == ',') ? ';' : ',';

    private static List<string> SplitLines(string text) =>
        [.. text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')];

    private static List<string> SplitLine(string line, char separator)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == separator)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }
}
