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
[Route("api/v1/company-contracts")]
[RequirePermission(PermissionCodes.FinancialDataView)]
public class CompanyContractsController(
    CompanyContractHistoryService service,
    IValidator<CompanyContractHistoryCreateRequest> createValidator,
    IValidator<CompanyContractHistoryUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CompanyContractHistoryDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? companyId, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, companyId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyContractHistoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyContractHistoryDto>> Create(
        [FromBody] CompanyContractHistoryCreateRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<CompanyContractHistoryDto>> Update(
        Guid id, [FromBody] CompanyContractHistoryUpdateRequest request, CancellationToken cancellationToken)
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
