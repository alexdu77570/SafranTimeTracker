using FluentValidation;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Applications.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout uniquement : <c>ApplicationReferenceService</c> n'expose pas de mise à jour à ce
/// jour.</summary>
public class ApplicationReferenceImportAdapter(
    ApplicationReferenceService service, IValidator<ApplicationReferenceCreateRequest> validator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Applications;

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Nom", "Code", "ServiceId", "TeamId", "Criticite", "ResponsableId", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<ApplicationReferenceCreateRequest>(row);
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Guid? newId = null;
        if (persist)
        {
            var dto = await service.CreateAsync(request, cancellationToken);
            newId = dto.Id;
        }

        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }
}
