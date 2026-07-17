using FluentValidation;
using SafranTimeTracker.Application.Orders.Dtos;

namespace SafranTimeTracker.Application.Orders.Validators;

public class OrderReceiptCreateRequestValidator : AbstractValidator<OrderReceiptCreateRequest>
{
    public OrderReceiptCreateRequestValidator()
    {
        RuleFor(x => x).Must(x => x.ReceivedAmount is not null || x.ReceivedDays is not null)
            .WithMessage("Une réception doit indiquer un montant ou un nombre de jours reçus.");
        RuleFor(x => x).Must(x => x.ReceivedAmount is null || x.ReceivedDays is null)
            .WithMessage("Une réception ne peut porter à la fois sur un montant et sur des jours.");
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
