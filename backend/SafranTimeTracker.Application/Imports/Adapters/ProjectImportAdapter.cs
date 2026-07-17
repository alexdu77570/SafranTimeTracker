using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout, Mise à jour et Complet (archivage via <c>ProjectService.ArchiveAsync</c>,
/// §16.3 — suppression physique interdite, CLAUDE.md §7).</summary>
public class ProjectImportAdapter(
    ProjectService service,
    IReadRepository<Project> readRepository,
    IReadRepository<ProjectStatus> statusRepository,
    IValidator<ProjectCreateRequest> createValidator,
    IValidator<ProjectUpdateRequest> updateValidator) : ImportAdapterBase
{
    private const string StatusActif = "ACTIF";

    public override ImportEntityType Type => ImportEntityType.Projects;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } =
        [ImportMode.Ajout, ImportMode.MiseAJour, ImportMode.Complet];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Nom", "Code", "ApplicationId", "DescriptionCourte", "PiloteId", "DepartmentId", "ServiceId", "TeamId",
         "DateDebut", "DateFinPrevueInitiale", "DateFinAjustee", "DateFinReelle", "BudgetInitial", "NiveauRisque", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<ProjectCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun projet existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<ProjectUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<ProjectDto>(), updateRequest);
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

    public override async Task<IReadOnlyList<Guid>> GetActiveIdsNotInAsync(IReadOnlyCollection<Guid> idsInFile, CancellationToken cancellationToken)
    {
        var activeStatusId = await statusRepository.Query().Where(s => s.Code == StatusActif).Select(s => s.Id).FirstAsync(cancellationToken);
        return await readRepository.Query()
            .Where(p => p.StatusId == activeStatusId && !idsInFile.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public override async Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken)
    {
        if (persist)
        {
            await service.ArchiveAsync(id, cancellationToken);
        }

        return new FieldChange("StatusId", StatusActif, "ARCHIVE");
    }
}
