using FluentValidation;
using Mapster;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout et Mise à jour. Pas de Complet : la machine d'état de la commande (§13.2,
/// inchangée depuis le Lot 5) ne se prête pas à un archivage "absent du fichier" — une commande
/// suit ses transitions dédiées, jamais un statut déduit d'un import. Société (CompanyId)
/// immuable après création, cohérent avec <see cref="OrderUpdateRequest"/> qui ne la porte pas.</summary>
public class OrderImportAdapter(
    OrderService service,
    IReadRepository<Order> readRepository,
    IValidator<OrderCreateRequest> createValidator,
    IValidator<OrderUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Orders;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout, ImportMode.MiseAJour];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Reference", "Libelle", "CompanyId", "ProjectId", "BudgetFinancierInitial", "BudgetJoursInitial",
         "DateDebut", "DateFinInitiale", "SeuilAlerte", "Commentaire", "AuthorizedResourceIds"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<OrderCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucune commande existante avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<OrderUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<OrderDto>(), updateRequest);
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
