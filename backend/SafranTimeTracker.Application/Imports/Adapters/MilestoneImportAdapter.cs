using FluentValidation;
using Mapster;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Application.Milestones.Services;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout et Mise à jour. Pas de Complet : "en retard" est dérivé à la lecture
/// (MilestoneDto.EstEnRetard) et le statut Annulé, seule notion proche d'un archivage, exige les
/// champs obligatoires (Nom/ResponsableId/DatePrevue) qu'un import ne doit pas réécrire à la
/// place de l'auteur d'origine — laissé hors périmètre de ce lot.</summary>
public class MilestoneImportAdapter(
    MilestoneService service,
    IReadRepository<Milestone> readRepository,
    IValidator<MilestoneCreateRequest> createValidator,
    IValidator<MilestoneUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Milestones;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Nom", "MilestoneTypeId", "ProjectId", "ApplicationId", "ResponsableId", "DatePrevue",
         "DateReelle", "Statut", "Criticite", "Commentaire", "DependsOnMilestoneId"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<MilestoneCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun jalon existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<MilestoneUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var oldDto = new MilestoneUpdateRequest
        {
            Nom = existing.Nom,
            ResponsableId = existing.ResponsableId,
            DatePrevue = existing.DatePrevue,
            DateReelle = existing.DateReelle,
            Statut = existing.Statut,
            Criticite = existing.Criticite,
            Commentaire = existing.Commentaire,
            DependsOnMilestoneId = existing.DependsOnMilestoneId
        };
        var changes = FieldDiffer.Diff(oldDto, updateRequest);
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
