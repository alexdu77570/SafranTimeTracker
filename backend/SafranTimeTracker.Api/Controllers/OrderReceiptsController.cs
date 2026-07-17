using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Réceptions partielles d'une commande (règle métier validée Lot 6), sous-ressource de
/// composition forte — même principe que orders/{id}/extensions (Lot 5).</summary>
[ApiController]
[Route("api/v1/orders/{orderId:guid}/receipts")]
public class OrderReceiptsController(OrderReceiptService service, IValidator<OrderReceiptCreateRequest> createValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderReceiptDto>>> GetList(
        Guid orderId, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(orderId, pagination, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderReceiptSummaryDto>> GetSummary(Guid orderId, CancellationToken cancellationToken)
    {
        var summary = await service.GetSummaryAsync(orderId, cancellationToken);
        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpPost]
    public async Task<ActionResult<OrderReceiptDto>> Create(
        Guid orderId, [FromBody] OrderReceiptCreateRequest request, CancellationToken cancellationToken)
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
