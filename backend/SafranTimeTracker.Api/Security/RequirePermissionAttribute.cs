using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Security;

/// <summary>
/// Garde de permission serveur (CLAUDE.md §12, §17) : ne dépend que de <see cref="ICurrentUser"/>,
/// jamais de ClaimsPrincipal ni du système d'authentification ASP.NET Core. En son absence, le
/// contrôleur n'est jamais atteint et aucune donnée financière n'est renvoyée — jamais un simple
/// masquage visuel côté client (docs/ARCHITECTURE.md §3).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePermissionAttribute(string permissionCode) : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (!currentUser.HasPermission(permissionCode))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Permission manquante",
                Detail = $"La permission '{permissionCode}' est requise pour accéder à cette ressource."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
