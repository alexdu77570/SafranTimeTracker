namespace SafranTimeTracker.Application.Common.Persistence;

/// <summary>Ajoute les opérations d'écriture à <see cref="IReadRepository{TEntity}"/>.</summary>
public interface IRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Suppression physique (Lot 6) : réservée aux relations de rattachement pur sans
    /// portée historique/métier (ex. UserPermission) — jamais utilisée pour une entité concernée
    /// par la règle "statut plutôt que suppression physique" (CLAUDE.md §7 : temps, absences,
    /// projets, imports, journaux). L'entité peut être un "stub" ne portant que sa clé.</summary>
    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
