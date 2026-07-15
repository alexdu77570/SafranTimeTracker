using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Application.Reporting.Services;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Référentiel administrable des KPI de tableau de bord (cahier des charges §30, §25) —
/// aucune donnée financière ici (seules les définitions, jamais les valeurs).</summary>
[ApiController]
[Route("api/v1/dashboard-kpis")]
public class DashboardKpisController(DashboardKpiService service, IValidator<DashboardKpiCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<DashboardKpiDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] DashboardKpiCategory? category, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, category, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DashboardKpiDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<DashboardKpiDto>> Create([FromBody] DashboardKpiCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
}
