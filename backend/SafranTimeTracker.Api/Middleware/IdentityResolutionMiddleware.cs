using Microsoft.Extensions.Options;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Middleware;

/// <summary>
/// Résout l'identité courante une seule fois par requête (session cookie, avec repli sur l'en-tête
/// <see cref="DemoCurrentUserProvider.DemoUserHeaderName"/> réservé à Development/Test — CLAUDE.md
/// §17, Lot 13) et calcule ses permissions effectives (<see cref="PermissionResolutionService"/>),
/// puis dépose le résultat dans <see cref="HttpContext.Items"/> sous <see cref="ItemsKey"/> pour que
/// <see cref="DemoCurrentUserProvider"/> (implémentation synchrone d'<see cref="ICurrentUser"/>,
/// inchangée dans sa forme) le lise sans jamais bloquer sur une tâche asynchrone.
/// </summary>
public sealed class IdentityResolutionMiddleware(RequestDelegate next)
{
    public const string ItemsKey = "SafranTimeTracker.ResolvedIdentity";
    public const string SessionCookieName = "stt_session";

    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationProvider authenticationProvider,
        PermissionResolutionService permissionResolutionService,
        IOptions<AuthenticationOptions> options)
    {
        var resolved = await ResolveAsync(context, authenticationProvider, options.Value);
        if (resolved is not null)
        {
            var permissionCodes = await permissionResolutionService.GetEffectivePermissionCodesAsync(resolved.UserId, context.RequestAborted);
            context.Items[ItemsKey] = new ResolvedCurrentIdentity(resolved.UserId, resolved.Identifiant, permissionCodes);
        }

        await next(context);
    }

    private static async Task<ResolvedIdentity?> ResolveAsync(
        HttpContext context, IAuthenticationProvider authenticationProvider, AuthenticationOptions options)
    {
        var cookieValue = context.Request.Cookies[SessionCookieName];
        if (cookieValue is not null && Guid.TryParse(cookieValue, out var sessionId))
        {
            var bySession = await authenticationProvider.ResolveSessionAsync(sessionId, context.RequestAborted);
            if (bySession is not null)
            {
                return bySession;
            }
        }

        if (!options.AllowDirectDemoHeader)
        {
            return null;
        }

        var headerValue = context.Request.Headers[DemoCurrentUserProvider.DemoUserHeaderName].FirstOrDefault();
        return string.IsNullOrWhiteSpace(headerValue)
            ? null
            : await authenticationProvider.ResolveDirectIdentifierAsync(headerValue, context.RequestAborted);
    }
}

/// <summary>Identité déjà résolue pour la requête courante, avec ses permissions effectives —
/// forme interne à l'Api, jamais exposée en dehors de <see cref="IdentityResolutionMiddleware"/> et
/// <see cref="DemoCurrentUserProvider"/>.</summary>
public sealed record ResolvedCurrentIdentity(Guid UserId, string Identifiant, IReadOnlyCollection<string> PermissionCodes);
