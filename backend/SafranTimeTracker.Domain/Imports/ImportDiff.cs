namespace SafranTimeTracker.Domain.Imports;

/// <summary>
/// Ligne de différence d'un <see cref="ImportBatch"/> (cahier des charges §27.6). Une ligne par
/// champ modifié pour une mise à jour (<see cref="ImportDiffType.Modification"/>), une ligne
/// unique par entité pour un ajout/une suppression logique.
/// </summary>
public class ImportDiff
{
    public Guid Id { get; set; }

    public Guid ImportBatchId { get; set; }
    public ImportBatch ImportBatch { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty;

    /// <summary>Null uniquement pour une ligne en erreur n'ayant pu être résolue à aucune entité.</summary>
    public Guid? EntityId { get; set; }

    public ImportDiffType DiffType { get; set; }

    /// <summary>Null pour un ajout/une suppression (diff au niveau de l'entité entière).</summary>
    public string? FieldName { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
