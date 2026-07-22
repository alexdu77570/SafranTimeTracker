using FluentValidation;
using SafranTimeTracker.Application.Auth.Dtos;

namespace SafranTimeTracker.Application.Auth.Validators;

public class AuthSessionRequestValidator : AbstractValidator<AuthSessionRequest>
{
    public AuthSessionRequestValidator()
    {
        RuleFor(x => x.Identifiant).NotEmpty().MaximumLength(50);
    }
}
