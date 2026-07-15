using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Application.Organisation.Services;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/teams")]
public class TeamsController(TeamService service, IValidator<TeamCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TeamDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? serviceId, [FromQuery] ReferentialStatus? statut, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, serviceId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create([FromBody] TeamCreateRequest request, CancellationToken cancellationToken)
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
