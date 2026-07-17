using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Audit.Dtos;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Consultation du journal d'audit (cahier des charges §28.1, §28.3 : "jamais modifiable
/// depuis l'interface standard" — aucune écriture exposée ici, voir AuditService). Endpoint entier
/// gardé par AUDIT_VIEW.</summary>
[ApiController]
[Route("api/v1/audit-logs")]
[RequirePermission(PermissionCodes.AuditView)]
public class AuditLogsController(AuditLogService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] string? author, [FromQuery] string? entityType,
        [FromQuery] string? action, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, author, entityType, action, from, to, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditLogDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
