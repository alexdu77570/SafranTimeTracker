using FluentValidation;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Application.Organisation.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports.Adapters;

/// <summary>
/// "Organisation" regroupe Department/Service/Team en un seul type importable (cahier des charges
/// §27.1, un seul item pour les trois) : la colonne <c>Niveau</c> ("Department"|"Service"|"Team")
/// choisit le service métier à appeler pour la ligne. Ajout uniquement : aucun des trois services
/// n'expose de mise à jour à ce jour.
/// </summary>
public class OrganisationImportAdapter(
    DepartmentService departmentService,
    ServiceService serviceService,
    TeamService teamService,
    IValidator<DepartmentCreateRequest> departmentValidator,
    IValidator<ServiceCreateRequest> serviceValidator,
    IValidator<TeamCreateRequest> teamValidator) : ImportAdapterBase
{
    public override ImportEntityType Type => ImportEntityType.Organisation;

    public override IReadOnlyList<string> ExpectedHeaders =>
        ["Niveau", "Code", "Nom", "DepartmentId", "ServiceId", "ResponsableId", "ResponsableFonctionnelId", "Commentaire"];

    public override async Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken)
    {
        var niveau = CsvRequestBinder.ReadOptional(row, "Niveau");
        switch (niveau?.Trim().ToUpperInvariant())
        {
            case "DEPARTMENT":
                return await ProcessDepartmentAsync(row, persist, cancellationToken);
            case "SERVICE":
                return await ProcessServiceAsync(row, persist, cancellationToken);
            case "TEAM":
                return await ProcessTeamAsync(row, persist, cancellationToken);
            default:
                return ImportRowOutcome.Error("La colonne 'Niveau' doit valoir 'Department', 'Service' ou 'Team'.");
        }
    }

    private async Task<ImportRowOutcome> ProcessDepartmentAsync(
        IReadOnlyDictionary<string, string> row, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<DepartmentCreateRequest>(row);
        var validation = await departmentValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Guid? newId = persist ? (await departmentService.CreateAsync(request, cancellationToken)).Id : null;
        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }

    private async Task<ImportRowOutcome> ProcessServiceAsync(
        IReadOnlyDictionary<string, string> row, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<ServiceCreateRequest>(row);
        var validation = await serviceValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Guid? newId = persist ? (await serviceService.CreateAsync(request, cancellationToken)).Id : null;
        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }

    private async Task<ImportRowOutcome> ProcessTeamAsync(
        IReadOnlyDictionary<string, string> row, bool persist, CancellationToken cancellationToken)
    {
        var request = CsvRequestBinder.Bind<TeamCreateRequest>(row);
        var validation = await teamValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ImportRowOutcome.Error(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Guid? newId = persist ? (await teamService.CreateAsync(request, cancellationToken)).Id : null;
        return new ImportRowOutcome { Success = true, EntityId = newId, DiffType = ImportDiffType.Ajout };
    }
}
