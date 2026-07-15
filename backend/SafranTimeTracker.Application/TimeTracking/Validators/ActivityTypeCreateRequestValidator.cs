using FluentValidation;
using SafranTimeTracker.Application.TimeTracking.Dtos;

namespace SafranTimeTracker.Application.TimeTracking.Validators;

public class ActivityTypeCreateRequestValidator : AbstractValidator<ActivityTypeCreateRequest>
{
    public ActivityTypeCreateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReferenceFormatRegex).MaximumLength(200);
        RuleFor(x => x.ReferenceExample).MaximumLength(50);
        RuleFor(x => x.ReferenceFormatRegex)
            .Must(BeAValidRegex)
            .When(x => !string.IsNullOrEmpty(x.ReferenceFormatRegex))
            .WithMessage("Le format de référence n'est pas une expression régulière valide.");
    }

    private static bool BeAValidRegex(string? pattern)
    {
        try
        {
            _ = new System.Text.RegularExpressions.Regex(pattern!);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
