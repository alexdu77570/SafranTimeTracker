using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Companies.Services;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/companies")]
public class CompaniesController(CompanyService service, IValidator<CompanyCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CompanyDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? companyTypeId, [FromQuery] ReferentialStatus? statut, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, companyTypeId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CompanyCreateRequest request, CancellationToken cancellationToken)
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
