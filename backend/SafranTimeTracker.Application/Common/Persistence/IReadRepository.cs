namespace SafranTimeTracker.Application.Common.Persistence;

/// <summary>
/// Abstraction de lecture définie côté Application, implémentée côté Infrastructure
/// (docs/ARCHITECTURE.md §2 : "interfaces de dépôt"). Expose IQueryable pour permettre
/// filtres/tri/pagination construits dans les services applicatifs, sans lier l'Application
/// à EF Core ni à un provider — choix pragmatique (CLAUDE.md §3) plutôt qu'un pattern
/// Specification, non justifié pour le périmètre actuel.
/// </summary>
public interface IReadRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
}
