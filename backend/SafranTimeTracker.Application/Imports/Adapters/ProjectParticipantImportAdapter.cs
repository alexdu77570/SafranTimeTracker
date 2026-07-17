using FluentValidation;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>
/// Ajout uniquement. <c>ProjectParticipantService</c> n'expose pas de mise à jour générique
/// (seulement Create/Remove). Pas de Complet non plus : contrairement aux types globaux (Project,
/// User, TimeEntry...), un participant est scopé à un projet (<c>ProjectId</c> en colonne, pas
/// dans <see cref="ProjectParticipantCreateRequest"/>) — un fichier ne décrivant qu'un seul projet
/// ne doit pas archiver les participants des autres projets, et le moteur d'import générique ne
/// porte pas de notion de "périmètre de fichier" pour le garantir en toute sécurité. Limitation
/// documentée plutôt que devinée.
/// </summary>
public class ProjectParticipantImportAdapter(
    ProjectParticipantService service, IValidator<ProjectParticipantCreateRequest> validator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.ProjectParticipants;

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["ProjectId", "ResourceId", "OperationalRoleId", "DefaultOrderId", "DateDebut", "DateFin", "CapacitePrevue"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var projectId = CsvRequestBinder.ReadRequiredGuid(row, "ProjectId");
        var request = CsvRequestBinder.Bind<ProjectParticipantCreateRequest>(row);

        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Guid? newId = null;
        if (persist)
        {
            var dto = await service.CreateAsync(projectId, request, cancellationToken);
            newId = dto.Id;
        }

        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }
}
