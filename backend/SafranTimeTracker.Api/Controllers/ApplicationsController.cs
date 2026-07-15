using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Applications.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Route conforme au cahier des charges §34 (/api/v1/applications) : seul le nom de
/// classe change côté implémentation pour éviter la collision de namespace (CLAUDE.md §6).</summary>
[ApiController]
[Route("api/v1/applications")]
public class ApplicationsController(ApplicationReferenceService service, IValidator<ApplicationReferenceCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ApplicationReferenceDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? serviceId, [FromQuery] ReferentialStatus? statut, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, serviceId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationReferenceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationReferenceDto>> Create([FromBody] ApplicationReferenceCreateRequest request, CancellationToken cancellationToken)
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
