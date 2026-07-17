using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;

namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>Implémentation générique de <see cref="IRepository{TEntity}"/> au-dessus d'EF Core.</summary>
public class EfRepository<TEntity>(AppDbContext dbContext) : IRepository<TEntity> where TEntity : class
{
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().FindAsync([id], cancellationToken).AsTask();

    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>().AsNoTracking();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
