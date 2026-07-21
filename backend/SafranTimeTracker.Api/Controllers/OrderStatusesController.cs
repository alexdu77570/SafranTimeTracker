using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/order-statuses")]
public class OrderStatusesController(OrderStatusService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderStatusDto>>> GetList(
        [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderStatusDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
