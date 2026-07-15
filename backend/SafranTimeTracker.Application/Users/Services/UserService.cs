using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users.Services;

public class UserService(IRepository<User> repository)
{
    public async Task<PagedResult<UserDto>> GetListAsync(
        PaginationQuery pagination, Guid? roleId, ReferentialStatus? statut, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (roleId is not null)
        {
            query = query.Where(u => u.RoleId == roleId);
        }
        if (statut is not null)
        {
            query = query.Where(u => u.Statut == statut);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.Nom).ThenBy(u => u.Prenom)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<UserDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(u => u.Id == id).ProjectToType<UserDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<UserDto> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entity = request.Adapt<User>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = now;
        entity.CreatedBy = CurrentActor.PlaceholderIdentifier;
        entity.SecurityLastModifiedAt = now;
        entity.SecurityLastModifiedBy = CurrentActor.PlaceholderIdentifier;
        entity.UserPermissions = request.PermissionIds
            .Select(permissionId => new UserPermission
            {
                UserId = entity.Id,
                PermissionId = permissionId,
                GrantedAt = now,
                GrantedBy = CurrentActor.PlaceholderIdentifier
            })
            .ToList();

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<UserDto>();
    }
}
