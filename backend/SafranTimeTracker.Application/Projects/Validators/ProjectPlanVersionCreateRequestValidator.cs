using FluentValidation;
using SafranTimeTracker.Application.Projects.Dtos;

namespace SafranTimeTracker.Application.Projects.Validators;

public class ProjectPlanVersionCreateRequestValidator : AbstractValidator<ProjectPlanVersionCreateRequest>
{
    public ProjectPlanVersionCreateRequestValidator()
    {
        RuleFor(x => x.Motif).MaximumLength(500);
    }
}

public class ProjectPlanVersionAdjustmentRequestValidator : AbstractValidator<ProjectPlanVersionAdjustmentRequest>
{
    public ProjectPlanVersionAdjustmentRequestValidator()
    {
        RuleFor(x => x.Motif).NotEmpty().MaximumLength(500)
            .WithMessage("Le motif est obligatoire pour créer une version ajustée (cahier des charges §18.3).");
    }
}
