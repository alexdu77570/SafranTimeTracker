using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Application.TimeTracking.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Le sous-objet financier de TimeEntryDto est omis sans FINANCIAL_DATA_VIEW,
/// projection faite par TimeEntryService (CLAUDE.md §13) — pas de garde au niveau contrôleur ici,
/// contrairement aux endpoints entièrement financiers du Lot 2.</summary>
[ApiController]
[Route("api/v1/time-entries")]
public class TimeEntriesController(
    TimeEntryService service,
    ITimeEntryRevaluationService revaluationService,
    IValidator<TimeEntryCreateRequest> createValidator,
    IValidator<TimeEntryUpdateRequest> updateValidator,
    IValidator<TimeEntryRecalculationRequest> recalculationValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TimeEntryDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? resourceId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] Guid? activityTypeId, [FromQuery] Guid? projectId, [FromQuery] Guid? orderId,
        CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, resourceId, from, to, activityTypeId, projectId, orderId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TimeEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> Create([FromBody] TimeEntryCreateRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<TimeEntryDto>> Update(Guid id, [FromBody] TimeEntryUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>§28.3 "suppression logique d'une saisie" : statut plutôt que suppression physique.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.DeleteAsync(id, cancellationToken);
        return dto is null ? NotFound() : NoContent();
    }

    /// <summary>§19.6 : recalcul explicite, permission dédiée, motif obligatoire.</summary>
    [HttpPost("{id:guid}/recalculate")]
    [RequirePermission(PermissionCodes.TimeEntryRecalculation)]
    public async Task<ActionResult> Recalculate(Guid id, [FromBody] TimeEntryRecalculationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await recalculationValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var snapshot = await revaluationService.RecalculateAsync(id, request.Reason, cancellationToken);
        return Ok(snapshot);
    }
}
