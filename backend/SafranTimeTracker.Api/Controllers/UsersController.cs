using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Application.Users.Services;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Les actions de sécurité (création, modification, désactivation, changement de rôle,
/// octroi/retrait de permission — cahier des charges §28.3) sont gardées par USER_ADMINISTRATION
/// (CLAUDE.md §17) ; seule la consultation reste ouverte, inchangée depuis le Lot 1.</summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController(
    UserService service,
    IValidator<UserCreateRequest> createValidator,
    IValidator<UserUpdateRequest> updateValidator,
    IValidator<RoleChangeRequest> roleChangeValidator) : ControllerBase
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
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
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
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/deactivate")]
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.DeactivateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/reactivate")]
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ReactivateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{id:guid}/role")]
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> ChangeRole(Guid id, [FromBody] RoleChangeRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await roleChangeValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.ChangeRoleAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/permissions/{permissionCode}")]
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> GrantPermission(Guid id, string permissionCode, CancellationToken cancellationToken)
    {
        var dto = await service.GrantPermissionAsync(id, permissionCode, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpDelete("{id:guid}/permissions/{permissionCode}")]
    [RequirePermission(PermissionCodes.UserAdministration)]
    public async Task<ActionResult<UserDto>> RevokePermission(Guid id, string permissionCode, CancellationToken cancellationToken)
    {
        var dto = await service.RevokePermissionAsync(id, permissionCode, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
