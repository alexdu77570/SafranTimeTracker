using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>
/// Cahier des charges §18.2 : vue "Planning projet" transverse à tous les projets. Agrégation
/// entièrement serveur (ProjectPlanningService.GetOverviewAsync) — décision actée avec l'utilisateur
/// à l'ouverture du Lot 10 pour remplacer une agrégation par N appels frontend, incompatible avec
/// une pagination/un tri serveur corrects.
/// </summary>
[ApiController]
[Route("api/v1/project-planning")]
public class ProjectPlanningOverviewController(ProjectPlanningService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectPlanningRowDto>>> GetOverview(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? projectId, [FromQuery] Guid? resourceId,
        [FromQuery] Guid? serviceId, [FromQuery] Guid? departmentId, [FromQuery] Guid? teamId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] bool? surcharge,
        CancellationToken cancellationToken)
    {
        var result = await service.GetOverviewAsync(
            pagination, projectId, resourceId, serviceId, departmentId, teamId, from, to, surcharge, cancellationToken);
        return Ok(result);
    }
}
