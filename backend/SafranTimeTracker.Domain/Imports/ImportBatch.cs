namespace SafranTimeTracker.Domain.Imports;

/// <summary>
/// Lot d'import (cahier des charges §27.5). Append-only : un aperçu/une simulation ne créent pas
/// de lot persistant (voir <c>ImportService</c>) ; seule une exécution confirmée écrit une ligne,
/// jamais corrigée ni supprimée ensuite (compte rendu figé).
/// </summary>
public class ImportBatch
{
    public Guid Id { get; set; }

    public ImportEntityType Type { get; set; }

    /// <summary>"CSV" ou "SharePoint" (§27.4, import SharePoint simulé — même pipeline, source
    /// différente).</summary>
    public string Source { get; set; } = string.Empty;

    public DateTime ImportDate { get; set; }

    /// <summary>Auteur (§27.5 "userId") : identifiant de l'appelant, même convention que
    /// <see cref="Auditing.AuditLog.Author"/>.</summary>
    public string UserId { get; set; } = string.Empty;

    public ImportMode Mode { get; set; }
    public string FileName { get; set; } = string.Empty;

    public int LineCount { get; set; }
    public int AddCount { get; set; }
    public int UpdateCount { get; set; }
    public int DeleteCount { get; set; }
    public int ErrorCount { get; set; }

    public ImportBatchStatus Status { get; set; }

    /// <summary>Erreurs sérialisées (JSON), une entrée par ligne en échec.</summary>
    public string? Errors { get; set; }

    /// <summary>Empreinte SHA-256 du contenu du fichier : détecte un rechargement identique et
    /// sert de repère pour la comparaison SharePoint simulée.</summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>Référence le dernier lot confirmé du même type/de la même source au moment de
    /// l'exécution (§27.4 "comparer au chargement précédent") — null si aucun lot antérieur.</summary>
    public Guid? PreviousBatchId { get; set; }

    public ICollection<ImportDiff> Diffs { get; set; } = new List<ImportDiff>();
}
