using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Resources.Services;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/resources")]
public class ResourcesController(ResourceService service, IValidator<ResourceCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ResourceDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? departmentId, [FromQuery] Guid? serviceId,
        [FromQuery] ReferentialStatus? statut, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, departmentId, serviceId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceDto>> Create([FromBody] ResourceCreateRequest request, CancellationToken cancellationToken)
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
