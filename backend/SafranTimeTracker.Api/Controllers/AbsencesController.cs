using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Absences.Services;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Domain.Absences;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/absences")]
public class AbsencesController(
    AbsenceService service,
    IValidator<AbsenceCreateRequest> createValidator,
    IValidator<AbsenceDecisionRequest> decisionValidator,
    IValidator<AbsenceUpdateRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AbsenceDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? resourceId, [FromQuery] AbsenceStatus? statut,
        CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, resourceId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AbsenceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<AbsenceDto>> Create([FromBody] AbsenceCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>§23.2 "modifier tant que permis" : restreint au statut Brouillon (409 sinon),
    /// docs/BACKLOG_METIER.md §12.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AbsenceDto>> Update(Guid id, [FromBody] AbsenceUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<AbsenceDto>> Submit(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.SubmitAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<ActionResult<AbsenceDto>> Validate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ValidateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/refuse")]
    public async Task<ActionResult<AbsenceDto>> Refuse(Guid id, [FromBody] AbsenceDecisionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await decisionValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.RefuseAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AbsenceDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.CancelAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
