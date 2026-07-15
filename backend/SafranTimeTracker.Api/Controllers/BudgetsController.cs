using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Budgets.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Ressource intégralement financière (cahier des charges §14) : endpoint entier gardé
/// par FINANCIAL_DATA_VIEW (403 sans permission), même principe que le Lot 2.</summary>
[ApiController]
[Route("api/v1/budgets")]
[RequirePermission(PermissionCodes.FinancialDataView)]
public class BudgetsController(
    BudgetService service,
    IValidator<BudgetCreateRequest> createValidator,
    IValidator<BudgetUpdateRequest> updateValidator,
    IValidator<BudgetAdjustRequest> adjustValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BudgetDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? projectId, [FromQuery] Guid? orderId, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, projectId, orderId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] BudgetCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, [FromBody] BudgetUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>§14.1 : pas de suppression physique, la clôture en tient lieu.</summary>
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<BudgetDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.CloseAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<BudgetDto>> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ReactivateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<ActionResult<PagedResult<BudgetVersionDto>>> GetVersions(
        Guid id, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetVersionsAsync(id, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>§14.2 : chaque ajustement conserve ancienne valeur, nouvelle valeur, motif, auteur, date.</summary>
    [HttpPost("{id:guid}/versions")]
    public async Task<ActionResult<BudgetVersionDto>> Adjust(Guid id, [FromBody] BudgetAdjustRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await adjustValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.AdjustAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : CreatedAtAction(nameof(GetVersions), new { id }, dto);
    }
}
