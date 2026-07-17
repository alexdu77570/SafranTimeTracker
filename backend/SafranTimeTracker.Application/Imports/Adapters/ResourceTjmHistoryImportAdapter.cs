using FluentValidation;
using Mapster;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout et Mise à jour (correction/clôture d'une période, §11.2). Pas de Complet :
/// entité historisée, une période "absente du fichier" n'a pas de sens à archiver
/// (docs/DATABASE.md §5).</summary>
public class ResourceTjmHistoryImportAdapter(
    ResourceTjmHistoryService service,
    IReadRepository<ResourceTjmHistory> readRepository,
    IValidator<ResourceTjmHistoryCreateRequest> createValidator,
    IValidator<ResourceTjmHistoryUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.ResourceTjmHistories;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "ResourceId", "StartDate", "EndDate", "DailyRate", "Reason", "Comment", "Status"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<ResourceTjmHistoryCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun historique TJM existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<ResourceTjmHistoryUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<ResourceTjmHistoryDto>(), updateRequest);
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
}
