using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Imports.Dtos;
using SafranTimeTracker.Application.Imports.Services;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Un seul paramètre complexe [FromForm] par action (plutôt que plusieurs paramètres
/// scalaires + IFormFile) : requis par Swashbuckle pour générer la documentation OpenAPI d'un
/// upload multipart (voir docs Swashbuckle "Handle forms and file uploads").</summary>
public class ImportPreviewForm
{
    public ImportEntityType Type { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class ImportSimulateForm
{
    public ImportEntityType Type { get; set; }
    public ImportMode Mode { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class ImportExecuteForm
{
    public ImportEntityType Type { get; set; }
    public ImportMode Mode { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? Source { get; set; }
}

/// <summary>
/// Cahier des charges §27 : aperçu (§27.3 étapes 1-4), simulation (étapes 5-9, aucune écriture),
/// exécution (étapes 10-12, écrit ImportBatch/ImportDiff et audite). Un import portant sur un type
/// financier (TJM, contrats, rattachements société, budgets, commandes) exige en plus
/// FINANCIAL_DATA_VIEW, même principe que les autres endpoints financiers (CLAUDE.md §12) — un
/// import n'est jamais une voie de contournement de la protection financière.
/// </summary>
[ApiController]
[Route("api/v1/imports")]
[RequirePermission(PermissionCodes.ImportExecute)]
public class ImportsController(ImportService service, ICurrentUser currentUser) : ControllerBase
{
    private static readonly HashSet<ImportEntityType> FinancialTypes =
    [
        ImportEntityType.ResourceTjmHistories,
        ImportEntityType.CompanyContractHistories,
        ImportEntityType.ResourceCompanyAssignments,
        ImportEntityType.Budgets,
        ImportEntityType.Orders
    ];

    [HttpGet("types")]
    public ActionResult<IReadOnlyList<ImportTypeMetadataDto>> GetTypes() => Ok(service.GetSupportedTypes());

    [HttpGet]
    public async Task<ActionResult<PagedResult<ImportBatchDto>>> GetBatches(
        [FromQuery] PaginationQuery pagination, [FromQuery] ImportEntityType? type, CancellationToken cancellationToken)
    {
        var result = await service.GetBatchesAsync(pagination, type, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ImportBatchDto>> GetBatchById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetBatchByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:guid}/diffs")]
    public async Task<ActionResult<PagedResult<ImportDiffDto>>> GetDiffs(
        Guid id, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetDiffsAsync(id, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpPost("preview")]
    public async Task<ActionResult<ImportPreviewDto>> Preview([FromForm] ImportPreviewForm form, CancellationToken cancellationToken)
    {
        var financialCheck = EnsureFinancialAccessIfNeeded(form.Type);
        if (financialCheck is not null)
        {
            return financialCheck;
        }

        var content = await ReadFileAsync(form.File, cancellationToken);
        return Ok(service.Preview(form.Type, content));
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<ImportSimulationDto>> Simulate([FromForm] ImportSimulateForm form, CancellationToken cancellationToken)
    {
        var financialCheck = EnsureFinancialAccessIfNeeded(form.Type);
        if (financialCheck is not null)
        {
            return financialCheck;
        }

        var content = await ReadFileAsync(form.File, cancellationToken);
        var result = await service.SimulateAsync(form.Type, form.Mode, content, cancellationToken);
        return Ok(result);
    }

    [HttpPost("execute")]
    public async Task<ActionResult<ImportBatchDto>> Execute([FromForm] ImportExecuteForm form, CancellationToken cancellationToken)
    {
        var financialCheck = EnsureFinancialAccessIfNeeded(form.Type);
        if (financialCheck is not null)
        {
            return financialCheck;
        }

        var content = await ReadFileAsync(form.File, cancellationToken);
        var result = await service.ExecuteAsync(form.Type, form.Mode, content, form.File.FileName, form.Source ?? "CSV", cancellationToken);
        return Ok(result);
    }

    /// <summary>§27.4 : import SharePoint simulé — même pipeline, source figée à "SharePoint".</summary>
    [HttpPost("sharepoint/execute")]
    public async Task<ActionResult<ImportBatchDto>> ExecuteSharePoint([FromForm] ImportSimulateForm form, CancellationToken cancellationToken)
    {
        var financialCheck = EnsureFinancialAccessIfNeeded(form.Type);
        if (financialCheck is not null)
        {
            return financialCheck;
        }

        var content = await ReadFileAsync(form.File, cancellationToken);
        var result = await service.ExecuteAsync(form.Type, form.Mode, content, form.File.FileName, "SharePoint", cancellationToken);
        return Ok(result);
    }

    private ActionResult? EnsureFinancialAccessIfNeeded(ImportEntityType type)
    {
        if (FinancialTypes.Contains(type) && !currentUser.HasPermission(PermissionCodes.FinancialDataView))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Permission manquante",
                Detail = $"La permission '{PermissionCodes.FinancialDataView}' est requise pour importer le type '{type}'."
            });
        }

        return null;
    }

    private static async Task<byte[]> ReadFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return stream.ToArray();
    }
}
