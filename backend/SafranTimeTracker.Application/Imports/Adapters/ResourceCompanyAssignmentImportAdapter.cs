using FluentValidation;
using Mapster;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout et Mise à jour (correction/clôture, §12.2). Pas de Complet : entité historisée
/// (docs/DATABASE.md §5).</summary>
public class ResourceCompanyAssignmentImportAdapter(
    ResourceCompanyAssignmentService service,
    IReadRepository<ResourceCompanyAssignment> readRepository,
    IValidator<ResourceCompanyAssignmentCreateRequest> createValidator,
    IValidator<ResourceCompanyAssignmentUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.ResourceCompanyAssignments;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "ResourceId", "CompanyId", "StartDate", "EndDate", "AssignmentType", "Comment", "Status"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<ResourceCompanyAssignmentCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun rattachement société existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<ResourceCompanyAssignmentUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<ResourceCompanyAssignmentDto>(), updateRequest);
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
