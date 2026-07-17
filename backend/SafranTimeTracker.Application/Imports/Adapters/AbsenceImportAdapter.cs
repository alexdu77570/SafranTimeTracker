using FluentValidation;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Absences.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout uniquement : <c>AbsenceService</c> n'expose que des transitions de workflow
/// dédiées (Submit/Validate/Refuse/Cancel, §23.3), pas de mise à jour générique de champ —
/// aucune n'est créée dans ce lot uniquement pour l'import.</summary>
public class AbsenceImportAdapter(AbsenceService service, IValidator<AbsenceCreateRequest> validator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Absences;

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["ResourceId", "Type", "DateDebut", "DateFin", "DemiJournee", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<AbsenceCreateRequest>(row);
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
