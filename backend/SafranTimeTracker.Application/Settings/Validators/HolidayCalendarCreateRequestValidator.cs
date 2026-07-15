using FluentValidation;
using SafranTimeTracker.Application.Settings.Dtos;

namespace SafranTimeTracker.Application.Settings.Validators;

public class HolidayCalendarCreateRequestValidator : AbstractValidator<HolidayCalendarCreateRequest>
{
    public HolidayCalendarCreateRequestValidator()
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Pays).NotEmpty().MaximumLength(50);
    }
}
