using Mapster;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Resources;

/// <summary>Mapping explicite entité ↔ DTO (CLAUDE.md §13) : OperationalRoles ne se
/// projette pas par convention (collection d'entités de jointure vers une liste d'identifiants).</summary>
public class ResourceMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Resource, ResourceDto>()
            .Map(dest => dest.OperationalRoleIds, src => src.OperationalRoles.Select(r => r.OperationalRoleId).ToList());
    }
}
