using FluentValidation;
using SafranTimeTracker.Application.Milestones.Dtos;

namespace SafranTimeTracker.Application.Milestones.Validators;

public class MilestoneTypeCreateRequestValidator : AbstractValidator<MilestoneTypeCreateRequest>
{
    public MilestoneTypeCreateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
    }
}
