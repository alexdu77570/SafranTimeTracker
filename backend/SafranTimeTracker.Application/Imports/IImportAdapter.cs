using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Application.Imports;

/// <summary>
/// Adaptateur de mapping propre à un type importable (cahier des charges §27.1). Réutilise
/// exclusivement les services métier applicatifs déjà écrits aux Lots 1 à 6 (Create/Update, ou
/// action dédiée) — jamais d'accès direct au contexte EF Core ni à <c>IRepository&lt;T&gt;</c>
/// d'écriture depuis cette couche (règle validée à l'ouverture du Lot 6) : le moteur d'import ne
/// duplique donc jamais une règle métier. <see cref="ProcessRowAsync"/> exécute exactement la même
/// validation/diff en aperçu, en simulation et à l'exécution (<paramref name="persist"/> distingue
/// uniquement l'appel réel au service d'écriture) — c'est ce qui garantit "ne pas modifier les
/// données avant confirmation" (§27.4).
/// </summary>
public interface IImportAdapter
{
    ImportEntityType Type { get; }

    /// <summary>En-têtes de colonnes attendues, exposées par <c>GET /api/v1/imports/types</c> —
    /// MVP sans assistant de correspondance interactif (§27.3 étape 5, différée au frontend).</summary>
    IReadOnlyList<string> ExpectedHeaders { get; }

    /// <summary>Modes réellement supportés par le service métier sous-jacent : Mise à jour n'est
    /// proposée que si une méthode Update existe déjà ; Complet uniquement si l'entité porte un
    /// mécanisme d'archivage logique (CLAUDE.md §7). Aucune nouvelle méthode de service n'est créée
    /// dans ce lot uniquement pour satisfaire l'import.</summary>
    IReadOnlyCollection<ImportMode> SupportedModes { get; }

    Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken);

    /// <summary>Mode Complet uniquement : entités actives non référencées par le fichier.</summary>
    Task<IReadOnlyList<Guid>> GetActiveIdsNotInAsync(IReadOnlyCollection<Guid> idsInFile, CancellationToken cancellationToken);

    /// <summary>Mode Complet uniquement : désactive/archive (jamais une suppression physique,
    /// CLAUDE.md §7).</summary>
    Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken);
}

/// <summary>Implémentation par défaut pour les types ne supportant que l'ajout (aucune méthode de
/// mise à jour/archivage sur le service métier sous-jacent).</summary>
public abstract class ImportAdapterBase : IImportAdapter
{
    public abstract ImportEntityType Type { get; }
    public abstract IReadOnlyList<string> ExpectedHeaders { get; }
    public virtual IReadOnlyCollection<ImportMode> SupportedModes { get; } = [ImportMode.Ajout];

    public abstract Task<ImportRowOutcome> ProcessRowAsync(
        IReadOnlyDictionary<string, string> row, ImportMode mode, bool persist, CancellationToken cancellationToken);

    public virtual Task<IReadOnlyList<Guid>> GetActiveIdsNotInAsync(IReadOnlyCollection<Guid> idsInFile, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public virtual Task<FieldChange?> ArchiveAsync(Guid id, bool persist, CancellationToken cancellationToken) =>
        throw new NotSupportedException($"Le mode Complet n'est pas supporté pour le type '{Type}'.");
}
