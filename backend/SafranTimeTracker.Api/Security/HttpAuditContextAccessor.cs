using SafranTimeTracker.Application.Audit.Services;

namespace SafranTimeTracker.Api.Security;

/// <summary>
/// Implémentation active de <see cref="IAuditContextAccessor"/> (CLAUDE.md §17, même principe que
/// <see cref="DemoCurrentUserProvider"/>) : lit l'adresse IP distante de la requête HTTP courante.
/// Seul ce fichier connaît <c>HttpContext</c> ; <c>AuditService</c> (Application) ne dépend que de
/// l'abstraction.
/// </summary>
public sealed class HttpAuditContextAccessor(IHttpContextAccessor httpContextAccessor) : IAuditContextAccessor
{
    public string? TechnicalContext => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
