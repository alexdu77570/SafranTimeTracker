using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Services;

/// <summary>
/// Génération réelle des exports (cahier des charges §26.3 : "les exports ne doivent pas être de
/// simples boutons simulés"). Ne calcule jamais de donnée métier : le contenu (ReportingTableDto)
/// est entièrement produit par ReportingService en amont — ExportService ne fait que restituer
/// dans le format demandé et journaliser (métadonnées minimales : date de génération, auteur,
/// version de l'application, filtres appliqués — demande explicite de l'utilisateur, Lot 5).
/// Un export financier (<paramref name="containsFinancialData"/> vrai) est en plus consigné dans
/// AuditLog (§28.3 "export financier") — un export opérationnel ne l'est pas, cette liste
/// n'attendant que l'export financier.
/// </summary>
public class ExportService(IRepository<ExportLog> exportLogRepository, AuditService auditService, ICurrentUser currentUser)
{
    private static readonly string AppVersion = typeof(ExportService).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    static ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ExportResultDto> ExportAsync(
        ReportingTableDto table, ExportFormat format, string reportType, object filters, bool containsFinancialData,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var generatedBy = currentUser.Identifier;
        var filtersJson = JsonSerializer.Serialize(filters);
        var metadata = $"Généré le {now:yyyy-MM-dd HH:mm} UTC par {generatedBy} — version {AppVersion}";

        var content = format switch
        {
            ExportFormat.Csv => BuildCsv(table, metadata, filtersJson),
            ExportFormat.Excel => BuildExcel(table, metadata, filtersJson),
            ExportFormat.Pdf => BuildPdf(table, metadata, filtersJson),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        var log = new ExportLog
        {
            Id = Guid.NewGuid(),
            GeneratedAt = now,
            GeneratedBy = generatedBy,
            AppVersion = AppVersion,
            ReportType = reportType,
            Format = format,
            FiltersJson = filtersJson,
            ContainsFinancialData = containsFinancialData
        };
        await exportLogRepository.AddAsync(log, cancellationToken);
        if (containsFinancialData)
        {
            await auditService.RecordAsync(
                AuditActions.ExportFinancial, reportType, null, null, new { format, filtersJson }, cancellationToken: cancellationToken);
        }
        await exportLogRepository.SaveChangesAsync(cancellationToken);

        return new ExportResultDto
        {
            Content = content,
            ContentType = format switch
            {
                ExportFormat.Csv => "text/csv",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.Pdf => "application/pdf",
                _ => "application/octet-stream"
            },
            FileName = $"{SanitizeFileName(reportType)}-{now:yyyyMMddHHmmss}.{format switch { ExportFormat.Csv => "csv", ExportFormat.Excel => "xlsx", ExportFormat.Pdf => "pdf", _ => "bin" }}"
        };
    }

    private static byte[] BuildCsv(ReportingTableDto table, string metadata, string filtersJson)
    {
        var builder = new StringBuilder();
        builder.AppendLine(EscapeCsvLine([table.Title]));
        builder.AppendLine(EscapeCsvLine([metadata]));
        builder.AppendLine(EscapeCsvLine([$"Filtres : {filtersJson}"]));
        builder.AppendLine();
        builder.AppendLine(EscapeCsvLine(table.Columns));
        foreach (var row in table.Rows)
        {
            builder.AppendLine(EscapeCsvLine(row));
        }

        return new UTF8Encoding(true).GetBytes(builder.ToString());
    }

    private static string EscapeCsvLine(IEnumerable<string> values) =>
        string.Join(',', values.Select(v => v.Contains(',') || v.Contains('"') || v.Contains('\n') ? $"\"{v.Replace("\"", "\"\"")}\"" : v));

    private static byte[] BuildExcel(ReportingTableDto table, string metadata, string filtersJson)
    {
        using var workbook = new XLWorkbook();
        var sheetName = table.Title.Length > 31 ? table.Title[..31] : table.Title;
        foreach (var invalidChar in new[] { ':', '\\', '/', '?', '*', '[', ']' })
        {
            sheetName = sheetName.Replace(invalidChar, '-');
        }
        var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Rapport" : sheetName);

        worksheet.Cell(1, 1).Value = table.Title;
        worksheet.Cell(2, 1).Value = metadata;
        worksheet.Cell(3, 1).Value = $"Filtres : {filtersJson}";

        const int headerRow = 5;
        for (var c = 0; c < table.Columns.Count; c++)
        {
            var cell = worksheet.Cell(headerRow, c + 1);
            cell.Value = table.Columns[c];
            cell.Style.Font.Bold = true;
        }

        for (var r = 0; r < table.Rows.Count; r++)
        {
            for (var c = 0; c < table.Rows[r].Length; c++)
            {
                worksheet.Cell(headerRow + 1 + r, c + 1).Value = table.Rows[r][c];
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(ReportingTableDto table, string metadata, string filtersJson)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4.Landscape());
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text(table.Title).FontSize(16).Bold();
                    column.Item().Text(metadata).FontSize(8);
                    column.Item().Text($"Filtres : {filtersJson}").FontSize(7);
                });

                page.Content().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in table.Columns)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    t.Header(header =>
                    {
                        foreach (var columnTitle in table.Columns)
                        {
                            header.Cell().Text(columnTitle).Bold();
                        }
                    });

                    foreach (var row in table.Rows)
                    {
                        foreach (var cell in row)
                        {
                            t.Cell().Text(cell);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());
    }
}
