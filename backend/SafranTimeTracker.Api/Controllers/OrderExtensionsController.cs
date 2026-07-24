using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Rallonges de commande (cahier des charges §13.3), sous-ressource de composition forte
/// (docs/CONVENTIONS.md §4) — même principe que projects/{id}/plan-versions (Lot 4). Création
/// gardée par FINANCIAL_DATA_VIEW (sous-lot 14.3 de l'audit du Lot 14, constat SEC-2) : augmente le
/// budget ajusté de la commande, même principe que les autres ressources intégralement financières
/// (Budgets, CompanyContracts, ResourceTjmHistory, ResourceCompanyAssignments).</summary>
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
    [RequirePermission(PermissionCodes.FinancialDataView)]
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
