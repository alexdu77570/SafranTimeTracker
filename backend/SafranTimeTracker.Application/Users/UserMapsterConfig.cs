using Mapster;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users;

/// <summary>Mapping explicite entité ↔ DTO (CLAUDE.md §13) : UserPermissions ne se
/// projette pas par convention (collection d'entités de jointure vers une liste d'identifiants).</summary>
public class UserMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.PermissionIds, src => src.UserPermissions.Select(p => p.PermissionId).ToList());
    }
}
