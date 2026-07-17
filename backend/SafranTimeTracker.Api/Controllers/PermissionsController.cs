using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Application.Users.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Référentiel en lecture seule des permissions (Lot 7) : permet au frontend de résoudre
/// UserDto.PermissionIds (GUIDs) en codes pour évaluer PermissionGuard. Aucune permission requise :
/// même principe d'ouverture que les autres petits référentiels (ex. ActivityTypesController).</summary>
[ApiController]
[Route("api/v1/permissions")]
public class PermissionsController(PermissionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PermissionDto>>> GetList(
        [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermissionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
