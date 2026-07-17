using FluentValidation;
using SafranTimeTracker.Application.TimeTracking.Dtos;

namespace SafranTimeTracker.Application.TimeTracking.Validators;

public class TimeEntryRecalculationRequestValidator : AbstractValidator<TimeEntryRecalculationRequest>
{
    public TimeEntryRecalculationRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500)
            .WithMessage("Le motif est obligatoire pour un recalcul explicite (cahier des charges §19.6).");
    }
}
