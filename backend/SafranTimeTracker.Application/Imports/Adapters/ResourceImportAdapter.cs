using FluentValidation;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Resources.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout uniquement : <c>ResourceService</c> n'expose pas de méthode de mise à jour ni
/// d'archivage à ce jour (aucune méthode n'est créée dans ce lot uniquement pour l'import,
/// principe validé à l'ouverture du Lot 6).</summary>
public class ResourceImportAdapter(ResourceService service, IValidator<ResourceCreateRequest> validator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Resources;

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Nom", "Prenom", "DepartmentId", "ServiceId", "TeamId", "ResponsableHierarchiqueId", "CompanyId",
         "DefaultOrderId", "DailyCapacity", "WeeklyCapacity", "Commentaire", "OperationalRoleIds"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<ResourceCreateRequest>(row);
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
