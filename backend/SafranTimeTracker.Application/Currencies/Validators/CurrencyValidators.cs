using FluentValidation;
using SafranTimeTracker.Application.Currencies.Dtos;

namespace SafranTimeTracker.Application.Currencies.Validators;

public class CurrencyCreateRequestValidator : AbstractValidator<CurrencyCreateRequest>
{
    public CurrencyCreateRequestValidator()
    {
        RuleFor(x => x.CodeIso).NotEmpty().Length(3).Matches("^[A-Z]{3}$")
            .WithMessage("Le code ISO doit contenir exactement 3 lettres majuscules (ISO 4217).");
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Symbole).NotEmpty().MaximumLength(5);
    }
}

/// <summary>Code ISO (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class CurrencyUpdateRequestValidator : AbstractValidator<CurrencyUpdateRequest>
{
    public CurrencyUpdateRequestValidator()
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Symbole).NotEmpty().MaximumLength(5);
    }
}
