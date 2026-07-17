using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Application.TimeTracking.Services;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Time;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout, Mise à jour et Complet (suppression logique via <c>TimeEntryService.DeleteAsync</c>,
/// §28.3). Chaque appel de service revalorise la saisie (§19.5), pas de recalcul dupliqué ici.</summary>
public class TimeEntryImportAdapter(
    TimeEntryService service,
    IReadRepository<TimeEntry> readRepository,
    IValidator<TimeEntryCreateRequest> createValidator,
    IValidator<TimeEntryUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.TimeEntries;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } =
        [ImportMode.Ajout, ImportMode.MiseAJour, ImportMode.Complet];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "ResourceId", "ActivityTypeId", "ProjectId", "OrderId", "Date", "DureeHeures", "Reference", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<TimeEntryCreateRequest>(row);
            var validation = await createValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
            }

            Guid? newId = persist ? (await service.CreateAsync(request, cancellationToken)).Id : null;
            return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
        }

        var existing = await readRepository.GetByIdAsync(id.Value, cancellationToken);
        if (existing is null)
        {
            return ImportRowOutcome.Error($"Aucune saisie de temps existante avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<TimeEntryUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<TimeEntryDto>(), updateRequest);
        if (changes.Count == 0)
        {
            return new ImportRowOutcome { Success = true, EntityId = id, DiffType = ImportDiffType.Inchange };
        }

        if (persist)
        {
            await service.UpdateAsync(id.Value, updateRequest, cancellationToken);
        }

        return new ImportRowOutcome { Success = true, EntityId = id, DiffType = ImportDiffType.Modification, Changes = changes };
    }

    public override async Task<IReadOnlyList<Guid>> GetActiveIdsNotInAsync(IReadOnlyCollection<Guid> idsInFile, CancellationToken cancellationToken) =>
        await readRepository.Query()
            .Where(t => t.Statut == ReferentialStatus.Actif && !idsInFile.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

    public override async Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken)
    {
        if (persist)
        {
            await service.DeleteAsync(id, cancellationToken);
        }

        return new FieldChange("Statut", nameof(ReferentialStatus.Actif), nameof(ReferentialStatus.Inactif));
    }
}
