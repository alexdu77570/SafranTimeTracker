using Mapster;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Orders;

namespace SafranTimeTracker.Application.Orders;

/// <summary>Mapping explicite entité ↔ DTO (CLAUDE.md §13) : AuthorizedResources ne se
/// projette pas par convention (collection d'entités de jointure vers une liste d'identifiants).</summary>
public class OrderMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderDto>()
            .Map(dest => dest.AuthorizedResourceIds, src => src.AuthorizedResources.Select(a => a.ResourceId).ToList());
    }
}
