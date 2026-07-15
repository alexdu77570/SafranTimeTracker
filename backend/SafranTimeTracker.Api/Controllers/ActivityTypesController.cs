using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Application.TimeTracking.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/activity-types")]
public class ActivityTypesController(ActivityTypeService service, IValidator<ActivityTypeCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ActivityTypeDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] bool? isRun, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, isRun, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityTypeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityTypeDto>> Create([FromBody] ActivityTypeCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
}
