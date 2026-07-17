using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Budgets.Services;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout, Mise à jour et Complet (clôture via <c>BudgetService.CloseAsync</c>, §14.1 —
/// pas de suppression physique, CLAUDE.md §7).</summary>
public class BudgetImportAdapter(
    BudgetService service,
    IReadRepository<Budget> readRepository,
    IValidator<BudgetCreateRequest> createValidator,
    IValidator<BudgetUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Budgets;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } =
        [ImportMode.Ajout, ImportMode.MiseAJour, ImportMode.Complet];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Name", "ProjectId", "OrderId", "InitialAmount", "AlertThreshold", "StartDate", "EndDate", "Comment"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<BudgetCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun budget existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<BudgetUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<BudgetDto>(), updateRequest);
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

    public override async Task<IReadOnlyList<Guid>> GetActiveIdsNotInAsync(IReadOnlyCollection<Guid> idsInFile, CancellationToken cancellationToken) =>
        await readRepository.Query()
            .Where(b => b.Status == BudgetStatus.Actif && !idsInFile.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

    public override async Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken)
    {
        if (persist)
        {
            await service.CloseAsync(id, cancellationToken);
        }

        return new FieldChange("Status", nameof(BudgetStatus.Actif), nameof(BudgetStatus.Cloture));
    }
}
