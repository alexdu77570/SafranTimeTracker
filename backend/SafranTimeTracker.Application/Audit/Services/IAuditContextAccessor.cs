namespace SafranTimeTracker.Application.Audit.Services;

/// <summary>
/// Contexte technique "si disponible" d'un enregistrement d'audit (cahier des charges §28.3).
/// Même principe que <c>ICurrentUser</c> (CLAUDE.md §17) : l'Application ne dépend que de cette
/// abstraction, jamais de <c>HttpContext</c> directement ; l'implémentation active
/// (<c>HttpAuditContextAccessor</c>, Api) lit l'adresse IP de l'appelant.
/// </summary>
public interface IAuditContextAccessor
{
    string? TechnicalContext { get; }
}
