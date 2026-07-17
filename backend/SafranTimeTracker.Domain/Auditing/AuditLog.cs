namespace SafranTimeTracker.Domain.Auditing;

/// <summary>
/// Journal d'audit métier (cahier des charges §28.3) : entité append-only, jamais modifiable
/// depuis l'interface standard, distincte du journal technique Serilog (CLAUDE.md §15-16).
/// L'écriture se fait toujours dans la même transaction que le changement métier qu'elle décrit
/// (voir <see cref="Application.Audit.Services.AuditService"/>, docs/ARCHITECTURE.md §2).
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }

    /// <summary>Auteur (§28.3 "userId") : identifiant de l'appelant (<c>ICurrentUser.Identifier</c>),
    /// une chaîne comme partout ailleurs (CreatedBy/UpdatedBy), jamais une clé étrangère vers
    /// <c>User</c> — cohérent avec l'authentification de démonstration (CLAUDE.md §17).</summary>
    public string Author { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    /// <summary>Code d'action stable, voir <see cref="AuditActions"/> (extensible sans migration
    /// de schéma, à la différence d'un enum fermé).</summary>
    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }

    /// <summary>Représentation textuelle (JSON) de l'ancienne valeur, null pour une création.</summary>
    public string? OldValue { get; set; }

    /// <summary>Représentation textuelle (JSON) de la nouvelle valeur, null pour une suppression logique.</summary>
    public string? NewValue { get; set; }

    public string? Reason { get; set; }

    /// <summary>Contexte technique "si disponible" (§28.3) : adresse IP de l'appelant, résolue via
    /// <c>IAuditContextAccessor</c> (Api) — jamais une dépendance directe à HttpContext depuis
    /// l'Application (CLAUDE.md §3).</summary>
    public string? TechnicalContext { get; set; }
}
