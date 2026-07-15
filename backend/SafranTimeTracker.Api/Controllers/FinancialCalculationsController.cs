using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Expose FinancialCalculationService pour prévisualisation/démonstration (cahier des
/// charges §20). N'écrit rien : le Lot 3 branchera ce même service sur TimeEntryService pour
/// figer un instantané réel.</summary>
[ApiController]
[Route("api/v1/financial-calculations")]
[RequirePermission(PermissionCodes.FinancialDataView)]
public class FinancialCalculationsController(
    FinancialCalculationService service, IValidator<FinancialCalculationRequest> validator) : ControllerBase
{
    [HttpPost("preview")]
    public async Task<ActionResult<FinancialCalculationResultDto>> Preview(
        [FromBody] FinancialCalculationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var result = await service.CalculateAsync(request, cancellationToken);
        return Ok(result);
    }
}
