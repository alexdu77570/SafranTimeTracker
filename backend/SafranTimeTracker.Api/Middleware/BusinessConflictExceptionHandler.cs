using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Exceptions;

namespace SafranTimeTracker.Api.Middleware;

/// <summary>
/// Traduit les conflits métier (chevauchement de périodes) et les conflits de concurrence
/// optimiste (TJM/contrat modifiés entretemps) en 409 ProblemDetails (CLAUDE.md §10, §12).
/// Toute autre exception continue de se propager normalement.
/// </summary>
public class BusinessConflictExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var detail = exception switch
        {
            BusinessConflictException businessConflictException => businessConflictException.Message,
            DbUpdateConcurrencyException => "La ressource a été modifiée entretemps par un autre appel ; rechargez-la avant de réessayer.",
            _ => null
        };

        if (detail is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflit métier",
            Detail = detail
        }, cancellationToken);

        return true;
    }
}
