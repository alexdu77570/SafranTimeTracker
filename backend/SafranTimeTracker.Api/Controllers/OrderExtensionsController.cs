using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Rallonges de commande (cahier des charges §13.3), sous-ressource de composition forte
/// (docs/CONVENTIONS.md §4) — même principe que projects/{id}/plan-versions (Lot 4).</summary>
[ApiController]
[Route("api/v1/orders/{orderId:guid}/extensions")]
public class OrderExtensionsController(
    OrderExtensionService service, IValidator<OrderExtensionCreateRequest> createValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderExtensionDto>>> GetList(
        Guid orderId, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(orderId, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderExtensionDto>> Create(
        Guid orderId, [FromBody] OrderExtensionCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(orderId, request, cancellationToken);
        return dto is null ? NotFound() : CreatedAtAction(nameof(GetList), new { orderId }, dto);
    }
}
