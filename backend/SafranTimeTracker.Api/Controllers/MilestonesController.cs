using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Application.Milestones.Services;
using SafranTimeTracker.Domain.Milestones;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/milestones")]
public class MilestonesController(
    MilestoneService service,
    IValidator<MilestoneCreateRequest> createValidator,
    IValidator<MilestoneUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<MilestoneDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? projectId, [FromQuery] Guid? responsableId,
        [FromQuery] MilestoneStatus? statut, [FromQuery] bool? enRetard, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, projectId, responsableId, statut, enRetard, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MilestoneDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<MilestoneDto>> Create([FromBody] MilestoneCreateRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<MilestoneDto>> Update(Guid id, [FromBody] MilestoneUpdateRequest request, CancellationToken cancellationToken)
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
