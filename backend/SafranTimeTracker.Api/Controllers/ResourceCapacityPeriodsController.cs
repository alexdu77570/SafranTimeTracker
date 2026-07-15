using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Capacity.Services;
using SafranTimeTracker.Application.Common.Dtos;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/resource-capacity-periods")]
public class ResourceCapacityPeriodsController(
    ResourceCapacityPeriodService service, IValidator<ResourceCapacityPeriodCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ResourceCapacityPeriodDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? resourceId, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, resourceId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceCapacityPeriodDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceCapacityPeriodDto>> Create(
        [FromBody] ResourceCapacityPeriodCreateRequest request, CancellationToken cancellationToken)
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
