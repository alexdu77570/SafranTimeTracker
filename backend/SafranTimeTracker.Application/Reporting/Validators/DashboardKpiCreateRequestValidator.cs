using FluentValidation;
using SafranTimeTracker.Application.Reporting.Dtos;

namespace SafranTimeTracker.Application.Reporting.Validators;

public class DashboardKpiCreateRequestValidator : AbstractValidator<DashboardKpiCreateRequest>
{
    public DashboardKpiCreateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Ordre).GreaterThanOrEqualTo(0);
    }
}
