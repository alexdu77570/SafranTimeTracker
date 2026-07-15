using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Application.Settings.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Ligne singleton (cahier des charges §28.2) : pas de liste, pas de création, uniquement
/// consultation et mise à jour.</summary>
[ApiController]
[Route("api/v1/settings")]
public class SettingsController(SettingsService service, IValidator<SettingsUpdateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SettingsDto>> Get(CancellationToken cancellationToken)
    {
        var dto = await service.GetAsync(cancellationToken);
        return Ok(dto);
    }

    [HttpPut]
    public async Task<ActionResult<SettingsDto>> Update([FromBody] SettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(request, cancellationToken);
        return Ok(dto);
    }
}
