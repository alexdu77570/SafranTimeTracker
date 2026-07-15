using FluentValidation.Results;

namespace SafranTimeTracker.Api.Extensions;

public static class ValidationResultExtensions
{
    /// <summary>Convertit les erreurs FluentValidation au format attendu par ValidationProblemDetails.</summary>
    public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result) =>
        result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
