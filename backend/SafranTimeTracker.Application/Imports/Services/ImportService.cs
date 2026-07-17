using System.Text.Json;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Exceptions;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Imports.Dtos;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Services;

/// <summary>
/// Orchestrateur unique du pipeline d'import (cahier des charges §27.3) : aperçu → simulation →
/// exécution, partagé par tous les types importables via <see cref="IImportAdapter"/>. N'accède
/// jamais directement au contexte EF Core pour les entités importées : chaque adaptateur réutilise
/// les services métier existants (règle validée à l'ouverture du Lot 6) ; seuls ImportBatch/
/// ImportDiff (propres au moteur d'import lui-même) sont écrits directement ici. Import SharePoint
/// simulé (§27.4) : même pipeline, <c>source = "SharePoint"</c>, comparaison au dernier lot
/// confirmé du même type/de la même source plutôt qu'à un fichier précédent stocké (état actuel de
/// la base = reflet du dernier import, pas de double stockage de fichiers bruts) — le point
/// d'extension pour un futur connecteur SharePoint réel est l'obtention des octets du fichier,
/// jamais la logique de comparaison/exécution ci-dessous.
/// </summary>
public class ImportService(
    IEnumerable<IImportAdapter> adapters,
    IRepository<ImportBatch> batchRepository,
    IRepository<ImportDiff> diffRepository,
    AuditService auditService,
    ICurrentUser currentUser)
{
    private const string SourceCsv = "CSV";
    private const string SourceSharePoint = "SharePoint";

    public IReadOnlyList<ImportTypeMetadataDto> GetSupportedTypes() =>
        adapters.Select(a => new ImportTypeMetadataDto
        {
            Type = a.Type,
            ExpectedHeaders = a.ExpectedHeaders,
            SupportedModes = [.. a.SupportedModes]
        }).OrderBy(t => t.Type).ToList();

    public ImportPreviewDto Preview(ImportEntityType type, byte[] fileContent)
    {
        var adapter = GetAdapter(type);
        var parsed = CsvFileParser.Parse(fileContent);

        return new ImportPreviewDto
        {
            DetectedHeaders = parsed.Headers,
            ExpectedHeaders = adapter.ExpectedHeaders,
            LineCount = parsed.Rows.Count,
            SampleRows = parsed.Rows.Take(10).ToList()
        };
    }

    public Task<ImportSimulationDto> SimulateAsync(
        ImportEntityType type, ImportMode mode, byte[] fileContent, CancellationToken cancellationToken = default) =>
        RunRowsAsync(type, mode, fileContent, persist: false, cancellationToken);

    /// <summary>§27.3 étapes 10-12 (confirmation/exécution/compte rendu) et §27.4 (import
    /// SharePoint simulé, <paramref name="source"/> = "SharePoint"). Écrit ImportBatch et
    /// ImportDiff, audite (§28.3 "import"/"comparaison d'import").</summary>
    public async Task<ImportBatchDto> ExecuteAsync(
        ImportEntityType type, ImportMode mode, byte[] fileContent, string fileName, string source = SourceCsv,
        CancellationToken cancellationToken = default)
    {
        var adapter = GetAdapter(type);
        EnsureModeSupported(adapter, mode);

        var simulation = await RunRowsAsync(type, mode, fileContent, persist: true, cancellationToken);
        var checksum = CsvFileParser.ComputeChecksum(fileContent);

        var previousBatchId = await batchRepository.Query()
            .Where(b => b.Type == type && b.Source == source && b.Status == ImportBatchStatus.Confirme)
            .OrderByDescending(b => b.ImportDate)
            .Select(b => (Guid?)b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            Type = type,
            Source = source,
            ImportDate = DateTime.UtcNow,
            UserId = currentUser.Identifier,
            Mode = mode,
            FileName = fileName,
            LineCount = simulation.LineCount,
            AddCount = simulation.AddCount,
            UpdateCount = simulation.UpdateCount,
            DeleteCount = simulation.DeleteCount,
            ErrorCount = simulation.ErrorCount,
            Status = simulation.LineCount > 0 && simulation.ErrorCount == simulation.LineCount
                ? ImportBatchStatus.Echoue
                : ImportBatchStatus.Confirme,
            Errors = simulation.ErrorCount == 0
                ? null
                : JsonSerializer.Serialize(simulation.Rows.Where(r => r.DiffType == ImportDiffType.Erreur)
                    .Select(r => new { r.RowNumber, r.ErrorMessage })),
            Checksum = checksum,
            PreviousBatchId = previousBatchId
        };

        await batchRepository.AddAsync(batch, cancellationToken);

        foreach (var rowResult in simulation.Rows.Where(r => r.DiffType is not (ImportDiffType.Inchange or ImportDiffType.Erreur)))
        {
            if (rowResult.Changes.Count == 0)
            {
                await diffRepository.AddAsync(new ImportDiff
                {
                    Id = Guid.NewGuid(),
                    ImportBatchId = batch.Id,
                    EntityType = type.ToString(),
                    EntityId = rowResult.EntityId,
                    DiffType = rowResult.DiffType
                }, cancellationToken);
            }
            else
            {
                foreach (var change in rowResult.Changes)
                {
                    await diffRepository.AddAsync(new ImportDiff
                    {
                        Id = Guid.NewGuid(),
                        ImportBatchId = batch.Id,
                        EntityType = type.ToString(),
                        EntityId = rowResult.EntityId,
                        DiffType = rowResult.DiffType,
                        FieldName = change.FieldName,
                        OldValue = change.OldValue,
                        NewValue = change.NewValue
                    }, cancellationToken);
                }
            }
        }

        await auditService.RecordAsync(
            AuditActions.Import, type.ToString(), batch.Id, null,
            new { batch.Mode, batch.Source, batch.AddCount, batch.UpdateCount, batch.DeleteCount, batch.ErrorCount },
            cancellationToken: cancellationToken);
        if (source == SourceSharePoint)
        {
            await auditService.RecordAsync(
                AuditActions.ImportComparison, type.ToString(), batch.Id, null,
                new { PreviousBatchId = previousBatchId }, cancellationToken: cancellationToken);
        }

        await batchRepository.SaveChangesAsync(cancellationToken);

        return batch.Adapt<ImportBatchDto>();
    }

    public async Task<PagedResult<ImportBatchDto>> GetBatchesAsync(
        PaginationQuery pagination, ImportEntityType? type, CancellationToken cancellationToken = default)
    {
        var query = batchRepository.Query();
        if (type is not null)
        {
            query = query.Where(b => b.Type == type);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.ImportDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ImportBatchDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ImportBatchDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<ImportBatchDto?> GetBatchByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        batchRepository.Query().Where(b => b.Id == id).ProjectToType<ImportBatchDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<ImportDiffDto>> GetDiffsAsync(
        Guid batchId, PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = diffRepository.Query().Where(d => d.ImportBatchId == batchId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<ImportDiffDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<ImportDiffDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    private async Task<ImportSimulationDto> RunRowsAsync(
        ImportEntityType type, ImportMode mode, byte[] fileContent, bool persist, CancellationToken cancellationToken)
    {
        var adapter = GetAdapter(type);
        EnsureModeSupported(adapter, mode);

        var parsed = CsvFileParser.Parse(fileContent);
        var rowResults = new List<ImportRowResultDto>(parsed.Rows.Count);
        var idsInFile = new HashSet<Guid>();
        var rowNumber = 1;

        foreach (var row in parsed.Rows)
        {
            rowNumber++;
            ImportRowOutcome outcome;
            try
            {
                outcome = await adapter.ProcessRowAsync(row, mode, persist, cancellationToken);
            }
            catch (Exception ex) when (ex is FormatException or BusinessConflictException or OverflowException or ArgumentException)
            {
                outcome = ImportRowOutcome.Error(ex.Message);
            }

            if (outcome.EntityId is not null)
            {
                idsInFile.Add(outcome.EntityId.Value);
            }

            rowResults.Add(new ImportRowResultDto
            {
                RowNumber = rowNumber,
                EntityId = outcome.EntityId,
                DiffType = outcome.DiffType,
                ErrorMessage = outcome.ErrorMessage,
                Changes = [.. outcome.Changes.Select(c => new FieldChangeDto { FieldName = c.FieldName, OldValue = c.OldValue, NewValue = c.NewValue })]
            });
        }

        var deleteCount = 0;
        if (mode == ImportMode.Complet)
        {
            var toArchive = await adapter.GetActiveIdsNotInAsync(idsInFile, cancellationToken);
            foreach (var id in toArchive)
            {
                var change = await adapter.ArchiveAsync(id, persist, cancellationToken);
                deleteCount++;
                rowResults.Add(new ImportRowResultDto
                {
                    RowNumber = 0,
                    EntityId = id,
                    DiffType = ImportDiffType.Suppression,
                    Changes = change is null ? [] : [new FieldChangeDto { FieldName = change.FieldName, OldValue = change.OldValue, NewValue = change.NewValue }]
                });
            }
        }

        return new ImportSimulationDto
        {
            LineCount = parsed.Rows.Count,
            AddCount = rowResults.Count(r => r.DiffType == ImportDiffType.Ajout),
            UpdateCount = rowResults.Count(r => r.DiffType == ImportDiffType.Modification),
            UnchangedCount = rowResults.Count(r => r.DiffType == ImportDiffType.Inchange),
            DeleteCount = deleteCount,
            ErrorCount = rowResults.Count(r => r.DiffType == ImportDiffType.Erreur),
            Rows = rowResults
        };
    }

    private IImportAdapter GetAdapter(ImportEntityType type) =>
        adapters.FirstOrDefault(a => a.Type == type)
            ?? throw new BusinessConflictException($"Aucun adaptateur d'import n'est enregistré pour le type '{type}'.");

    private static void EnsureModeSupported(IImportAdapter adapter, ImportMode mode)
    {
        if (!adapter.SupportedModes.Contains(mode))
        {
            throw new BusinessConflictException(
                $"Le mode '{mode}' n'est pas supporté pour le type '{adapter.Type}' (modes disponibles : "
                + $"{string.Join(", ", adapter.SupportedModes)}).");
        }
    }
}
