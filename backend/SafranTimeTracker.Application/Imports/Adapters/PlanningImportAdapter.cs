using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>
/// Une ligne = une entrée de planning hebdomadaire (§18.3). <c>ProjectPlanningService.SetWeeklyPlansAsync</c>
/// (Lot 4) fait déjà l'upsert par (ProjectPlanVersionId, ResourceId, WeekStartDate) : appelé ici
/// avec une liste d'un seul élément par ligne pour conserver la granularité par ligne du moteur
/// d'import (diff/erreur individuels), sans dupliquer sa logique de recherche. Ajout et Mise à
/// jour confondus (le service ne distingue pas les deux, tout comme son usage normal via l'API) ;
/// pas de Complet, même limitation de périmètre que ProjectParticipants (une ligne ne porte pas la
/// notion de "toutes les semaines d'une version").
/// </summary>
public class PlanningImportAdapter(ProjectPlanningService service, IReadRepository<ProjectWeeklyPlan> readRepository) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Plannings;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders => ["ProjectId", "ProjectPlanVersionId", "ResourceId", "WeekStartDate", "ChargePlanifieeHeures"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        Guid versionId, resourceId;
        DateOnly weekStartDate;
        decimal chargeHeures;
        try
        {
            versionId = CsvRequestBinder.ReadRequiredGuid(row, "ProjectPlanVersionId");
            resourceId = CsvRequestBinder.ReadRequiredGuid(row, "ResourceId");
            weekStartDate = CsvRequestBinder.ReadRequiredDate(row, "WeekStartDate");
            chargeHeures = CsvRequestBinder.ReadRequiredDecimal(row, "ChargePlanifieeHeures");
        }
        catch (FormatException ex)
        {
            return ImportRowOutcome.Error(ex.Message);
        }

        var existing = await readRepository.Query()
            .Where(w => w.ProjectPlanVersionId == versionId && w.ResourceId == resourceId && w.WeekStartDate == weekStartDate)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var line = new ProjectWeeklyPlanLineRequest { ResourceId = resourceId, WeekStartDate = weekStartDate, ChargePlanifieeHeures = chargeHeures };

        if (existing is not null)
        {
            var currentCharge = await readRepository.GetByIdAsync(existing.Value, cancellationToken);
            if (currentCharge?.ChargePlanifieeHeures == chargeHeures)
            {
                return new ImportRowOutcome { Success = true, EntityId = existing, DiffType = ImportDiffType.Inchange };
            }

            if (persist)
            {
                await service.SetWeeklyPlansAsync(versionId, [line], cancellationToken);
            }

            return new ImportRowOutcome
            {
                Success = true,
                EntityId = existing,
                DiffType = ImportDiffType.Modification,
                Changes = [new FieldChange("ChargePlanifieeHeures", currentCharge?.ChargePlanifieeHeures.ToString(), chargeHeures.ToString())]
            };
        }

        Guid? newId = null;
        if (persist)
        {
            var results = await service.SetWeeklyPlansAsync(versionId, [line], cancellationToken);
            newId = results.FirstOrDefault()?.Id;
        }

        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }
}
