using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Financial.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/resource-tjm-history")]
[RequirePermission(PermissionCodes.FinancialDataView)]
public class ResourceTjmHistoryController(
    ResourceTjmHistoryService service,
    IValidator<ResourceTjmHistoryCreateRequest> createValidator,
    IValidator<ResourceTjmHistoryUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ResourceTjmHistoryDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? resourceId, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, resourceId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceTjmHistoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceTjmHistoryDto>> Create(
        [FromBody] ResourceTjmHistoryCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResourceTjmHistoryDto>> Update(
        Guid id, [FromBody] ResourceTjmHistoryUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
