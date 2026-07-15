using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Application.Users.Services;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(UserService service, IValidator<UserCreateRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? roleId, [FromQuery] ReferentialStatus? statut, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, roleId, statut, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
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
