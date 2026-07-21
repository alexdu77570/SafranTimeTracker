using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>
/// Versions de planning (§18.3) et synthèse écarts/risques (§18.1, §29.5). RisqueBudget, seul
/// champ financier de la synthèse, est omis sans FINANCIAL_DATA_VIEW au niveau champ
/// (ProjectPlanningService), pas au niveau contrôleur.
/// </summary>
[ApiController]
[Route("api/v1/projects/{projectId:guid}")]
public class ProjectPlanningController(
    ProjectPlanningService service,
    IValidator<ProjectPlanVersionCreateRequest> initialValidator,
    IValidator<ProjectPlanVersionAdjustmentRequest> adjustmentValidator,
    IValidator<ProjectWeeklyPlanLineRequest> weeklyPlanLineValidator) : ControllerBase
{
    [HttpGet("plan-versions")]
    public async Task<ActionResult<PagedResult<ProjectPlanVersionDto>>> GetVersions(
        Guid projectId, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetVersionsAsync(projectId, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpPost("plan-versions/initial")]
    public async Task<ActionResult<ProjectPlanVersionDto>> CreateInitial(
        Guid projectId, [FromBody] ProjectPlanVersionCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await initialValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateInitialVersionAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetVersions), new { projectId }, dto);
    }

    /// <summary>Archive automatiquement la version Ajustée Active précédente, s'il en existe une
    /// (précision actée avec l'utilisateur, Lot 4).</summary>
    [HttpPost("plan-versions/adjusted")]
    public async Task<ActionResult<ProjectPlanVersionDto>> CreateAdjusted(
        Guid projectId, [FromBody] ProjectPlanVersionAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await adjustmentValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAdjustedVersionAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetVersions), new { projectId }, dto);
    }

    [HttpPost("plan-versions/{versionId:guid}/weekly-plans")]
    public async Task<ActionResult<IReadOnlyList<ProjectWeeklyPlanDto>>> SetWeeklyPlans(
        Guid projectId, Guid versionId, [FromBody] List<ProjectWeeklyPlanLineRequest> lines, CancellationToken cancellationToken)
    {
        foreach (var line in lines)
        {
            var validationResult = await weeklyPlanLineValidator.ValidateAsync(line, cancellationToken);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }
        }

        var result = await service.SetWeeklyPlansAsync(versionId, lines, cancellationToken);
        return Ok(result);
    }

    [HttpGet("plan-versions/{versionId:guid}/weekly-plans")]
    public async Task<ActionResult<IReadOnlyList<ProjectWeeklyPlanDto>>> GetWeeklyPlans(
        Guid projectId, Guid versionId, CancellationToken cancellationToken)
    {
        var result = await service.GetWeeklyPlansAsync(versionId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("planning")]
    public async Task<ActionResult<ProjectPlanningSynthesisDto>> GetSynthesis(Guid projectId, CancellationToken cancellationToken)
    {
        var dto = await service.GetSynthesisAsync(projectId, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
