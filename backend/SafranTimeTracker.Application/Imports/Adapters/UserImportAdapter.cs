using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Application.Users.Services;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>Ajout, Mise à jour et Complet (désactivation via <c>UserService.DeactivateAsync</c>,
/// §28.3). Rôle et permissions ne sont volontairement pas importables ici, même règle que
/// <see cref="UserUpdateRequest"/> — ce sont des actions dédiées et auditées séparément
/// (changement de rôle/permission), pas un champ d'import de masse.</summary>
public class UserImportAdapter(
    UserService service,
    IReadRepository<User> readRepository,
    IValidator<UserCreateRequest> createValidator,
    IValidator<UserUpdateRequest> updateValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Users;

    public override IReadOnlyCollection<ImportMode> SupportedModes { get; } =
        [ImportMode.Ajout, ImportMode.MiseAJour, ImportMode.Complet];

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Id", "Nom", "Prenom", "Identifiant", "Email", "Telephone", "DateArrivee", "Commentaire", "ResourceId", "RoleId"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var id = CsvRequestBinder.ReadOptionalId(row);
        if (id is null)
        {
            var request = CsvRequestBinder.Bind<UserCreateRequest>(row);
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
            return ImportRowOutcome.Error($"Aucun utilisateur existant avec l'identifiant '{id}'.");
        }

        var updateRequest = CsvRequestBinder.Bind<UserUpdateRequest>(row);
        var updateValidation = await updateValidator.ValidateAsync(updateRequest, cancellationToken);
        if (!updateValidation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", updateValidation.Errors.Select(e => e.ErrorMessage)));
        }

        var changes = FieldDiffer.Diff(existing.Adapt<UserDto>(), updateRequest);
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
            .Where(u => u.Statut == ReferentialStatus.Actif && !idsInFile.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

    public override async Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken)
    {
        if (persist)
        {
            await service.DeactivateAsync(id, cancellationToken);
        }

        return new FieldChange("Statut", nameof(ReferentialStatus.Actif), nameof(ReferentialStatus.Inactif));
    }
}
