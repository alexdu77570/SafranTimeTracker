using FluentValidation;
using Mapster;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Companies.Services;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout et Mise à jour (id en colonne 'Id' = mise à jour) : pas de Complet, aucun
/// mécanisme d'archivage n'existe sur <c>CompanyService</c> (Code, clé métier, n'est jamais
/// modifiable — même règle que <see cref="CompanyUpdateRequest"/>).</summary>
public class CompanyImportAdapter(
    CompanyService service,
    IReadRepository<Company> readRepository,
    IValidator<CompanyCreateRequest> createValidator,
    IValidator<CompanyUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Companies;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Nom", "Code", "CompanyTypeId", "ContactPrincipal", "EmailContact", "Telephone", "Adresse", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<CompanyCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucune société existante avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<CompanyUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<CompanyDto>(), updateRequest);
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
