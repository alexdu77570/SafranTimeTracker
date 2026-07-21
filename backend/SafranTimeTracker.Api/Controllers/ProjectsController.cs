using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Le sous-objet financier de ProjectDto est omis sans FINANCIAL_DATA_VIEW, projection
/// faite par ProjectService (CLAUDE.md §13) — pas de garde au niveau contrôleur, ce projet n'est
/// pas une ressource intégralement financière (contrairement aux endpoints du Lot 2).</summary>
[ApiController]
[Route("api/v1/projects")]
public class ProjectsController(
    ProjectService service,
    IValidator<ProjectCreateRequest> createValidator,
    IValidator<ProjectUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? statusId, [FromQuery] Guid? applicationId,
        [FromQuery] Guid? piloteId, [FromQuery] Guid? departmentId, [FromQuery] Guid? serviceId,
        [FromQuery] Guid? teamId, [FromQuery] ProjectRiskLevel? niveauRisque,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] bool? alertePlanning, [FromQuery] bool? alerteBudget,
        CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(
            pagination, statusId, applicationId, piloteId, departmentId, serviceId,
            teamId, niveauRisque, from, to, alertePlanning, alerteBudget, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] ProjectCreateRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] ProjectUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>§16.3 : la suppression n'est pas exposée (CLAUDE.md §7), l'archivage en tient lieu.</summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ProjectDto>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ArchiveAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<ProjectDto>> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ReactivateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
