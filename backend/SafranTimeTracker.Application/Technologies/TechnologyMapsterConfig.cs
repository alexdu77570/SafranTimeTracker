using Mapster;
using SafranTimeTracker.Application.Technologies.Dtos;
using SafranTimeTracker.Domain.Technologies;

namespace SafranTimeTracker.Application.Technologies;

/// <summary>Mapping explicite entité ↔ DTO (CLAUDE.md §13) : Applications/Resources ne se
/// projettent pas par convention (collections d'entités de jointure vers des listes d'identifiants).</summary>
public class TechnologyMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Technology, TechnologyDto>()
            .Map(dest => dest.ApplicationIds, src => src.Applications.Select(a => a.ApplicationId).ToList())
            .Map(dest => dest.ResourceIds, src => src.Resources.Select(r => r.ResourceId).ToList());
    }
}
