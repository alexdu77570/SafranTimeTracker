using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Sous-ressource de composition forte (docs/CONVENTIONS.md §4, cahier des charges §17.2).</summary>
[ApiController]
[Route("api/v1/projects/{projectId:guid}/participants")]
public class ProjectParticipantsController(
    ProjectParticipantService service, IValidator<ProjectParticipantCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectParticipantDto>>> GetList(
        Guid projectId, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(projectId, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectParticipantDto>> Create(
        Guid projectId, [FromBody] ProjectParticipantCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetList), new { projectId }, dto);
    }

    /// <summary>Retrait (désactivation, CLAUDE.md §7) — pas une suppression physique.</summary>
    [HttpDelete("{participantId:guid}")]
    public async Task<ActionResult<ProjectParticipantDto>> Remove(Guid projectId, Guid participantId, CancellationToken cancellationToken)
    {
        var dto = await service.RemoveAsync(projectId, participantId, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
