using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Middleware;
using SafranTimeTracker.Application.Auth.Dtos;
using SafranTimeTracker.Application.Common.Security;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>
/// Authentification simulée sessionnée (CLAUDE.md §17, Lot 13) : remplace le rejeu de l'en-tête
/// X-Demo-User par une session serveur (cookie HttpOnly opaque). Aucune permission requise — ce
/// sont les actions de connexion/déconnexion elles-mêmes, ouvertes par nature.
/// </summary>
[ApiController]
[Route("api/v1/auth/sessions")]
public class AuthController(
    IAuthenticationProvider authenticationProvider,
    IValidator<AuthSessionRequest> validator,
    IHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthSessionDto>> Create([FromBody] AuthSessionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var session = await authenticationProvider.CreateSessionAsync(request.Identifiant, request.RememberMe, cancellationToken);
        if (session is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Identifiant inconnu ou inactif",
                Detail = "Aucun utilisateur actif ne correspond à cet identifiant."
            });
        }

        Response.Cookies.Append(IdentityResolutionMiddleware.SessionCookieName, session.SessionId.ToString(), CookieOptionsFor(session.IsPersistent, session.ExpiresAt));

        return Ok(new AuthSessionDto
        {
            UserId = session.UserId,
            Identifiant = session.Identifiant,
            ExpiresAt = session.ExpiresAt,
            IsPersistent = session.IsPersistent
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(IdentityResolutionMiddleware.SessionCookieName, out var cookieValue)
            && Guid.TryParse(cookieValue, out var sessionId))
        {
            await authenticationProvider.RevokeSessionAsync(sessionId, cancellationToken);
        }

        Response.Cookies.Delete(IdentityResolutionMiddleware.SessionCookieName);
        return NoContent();
    }

    private CookieOptions CookieOptionsFor(bool isPersistent, DateTime expiresAt) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = !environment.IsDevelopment(),
        // Session navigateur (IsPersistent = false) : pas d'expiration explicite côté cookie, le
        // navigateur l'efface à sa fermeture ; le serveur reste la seule autorité (expiration
        // glissante sur UserSession). Session persistante : expiration explicite alignée sur celle
        // du serveur, pour permettre au navigateur de la conserver entre redémarrages.
        Expires = isPersistent ? expiresAt : null
    };
}
