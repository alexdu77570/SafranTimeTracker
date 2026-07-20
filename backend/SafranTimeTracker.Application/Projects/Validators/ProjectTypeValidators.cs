using FluentValidation;
using SafranTimeTracker.Application.Projects.Dtos;

namespace SafranTimeTracker.Application.Projects.Validators;

public class ProjectTypeCreateRequestValidator : AbstractValidator<ProjectTypeCreateRequest>
{
    public ProjectTypeCreateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
    }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class ProjectTypeUpdateRequestValidator : AbstractValidator<ProjectTypeUpdateRequest>
{
    public ProjectTypeUpdateRequestValidator()
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
    }
}
