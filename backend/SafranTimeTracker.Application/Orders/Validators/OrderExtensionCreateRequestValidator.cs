using FluentValidation;
using SafranTimeTracker.Application.Orders.Dtos;

namespace SafranTimeTracker.Application.Orders.Validators;

public class OrderExtensionCreateRequestValidator : AbstractValidator<OrderExtensionCreateRequest>
{
    public OrderExtensionCreateRequestValidator()
    {
        RuleFor(x => x.AmountAdded).GreaterThan(0);
        RuleFor(x => x.DaysAdded).GreaterThan(0).When(x => x.DaysAdded is not null);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
