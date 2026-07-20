using FluentValidation;
using SafranTimeTracker.Application.Clients.Dtos;

namespace SafranTimeTracker.Application.Clients.Validators;

public class ClientCreateRequestValidator : AbstractValidator<ClientCreateRequest>
{
    public ClientCreateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);
    }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class ClientUpdateRequestValidator : AbstractValidator<ClientUpdateRequest>
{
    public ClientUpdateRequestValidator()
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);
    }
}
