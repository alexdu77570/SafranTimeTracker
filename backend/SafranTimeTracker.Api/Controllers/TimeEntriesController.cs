using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
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
    IValidator<TimeEntryCreateRequest> createValidator,
    IValidator<TimeEntryUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TimeEntryDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? resourceId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, resourceId, from, to, cancellationToken);
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
}
