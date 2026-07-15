using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Capacity.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Sous-ressource de composition forte (docs/CONVENTIONS.md §4) : la disponibilité n'a
/// de sens que pour une ressource et une période données.</summary>
[ApiController]
[Route("api/v1/resources/{resourceId:guid}/availability")]
public class AvailabilityController(AvailabilityService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AvailabilityResultDto>> Get(
        Guid resourceId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, CancellationToken cancellationToken)
    {
        var result = await service.GetAvailabilityAsync(resourceId, startDate, endDate, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
