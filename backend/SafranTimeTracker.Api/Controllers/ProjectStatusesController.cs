using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Projects.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/project-statuses")]
public class ProjectStatusesController(ProjectStatusService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectStatusDto>>> GetList(
        [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectStatusDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
