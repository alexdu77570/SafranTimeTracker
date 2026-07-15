using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Application.Settings.Services;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/holiday-calendar")]
public class HolidayCalendarController(HolidayCalendarService service, IValidator<HolidayCalendarCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<HolidayCalendarDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, year, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HolidayCalendarDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<HolidayCalendarDto>> Create([FromBody] HolidayCalendarCreateRequest request, CancellationToken cancellationToken)
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
