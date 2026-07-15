using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Application.Milestones.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/milestone-types")]
public class MilestoneTypesController(MilestoneTypeService service, IValidator<MilestoneTypeCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<MilestoneTypeDto>>> GetList(
        [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MilestoneTypeDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<MilestoneTypeDto>> Create([FromBody] MilestoneTypeCreateRequest request, CancellationToken cancellationToken)
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
