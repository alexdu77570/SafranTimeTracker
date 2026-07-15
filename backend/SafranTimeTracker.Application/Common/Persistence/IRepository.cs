namespace SafranTimeTracker.Application.Common.Persistence;

/// <summary>Ajoute les opérations d'écriture à <see cref="IReadRepository{TEntity}"/>.</summary>
public interface IRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
