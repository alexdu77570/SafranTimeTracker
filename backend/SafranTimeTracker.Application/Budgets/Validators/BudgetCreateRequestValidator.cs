using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Budgets.Validators;

public class BudgetCreateRequestValidator : AbstractValidator<BudgetCreateRequest>
{
    public BudgetCreateRequestValidator(IReadRepository<Project> projectRepository, IReadRepository<Order> orderRepository)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InitialAmount).GreaterThan(0);
        RuleFor(x => x.AlertThreshold).InclusiveBetween(0, 100).When(x => x.AlertThreshold is not null);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Comment).MaximumLength(1000);

        RuleFor(x => x)
            .Must(x => x.ProjectId is not null || x.OrderId is not null)
            .WithMessage("Un budget doit être lié à un projet et/ou une commande (cahier des charges §14.1).");

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .When(x => x.ProjectId is not null)
            .WithMessage("Le projet indiqué n'existe pas.");

        RuleFor(x => x.OrderId)
            .MustAsync(async (id, ct) => await orderRepository.Query().AnyAsync(o => o.Id == id, ct))
            .When(x => x.OrderId is not null)
            .WithMessage("La commande indiquée n'existe pas.");
    }
}

public class BudgetUpdateRequestValidator : AbstractValidator<BudgetUpdateRequest>
{
    public BudgetUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AlertThreshold).InclusiveBetween(0, 100).When(x => x.AlertThreshold is not null);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class BudgetAdjustRequestValidator : AbstractValidator<BudgetAdjustRequest>
{
    public BudgetAdjustRequestValidator()
    {
        RuleFor(x => x.NewValue).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReferencePiece).MaximumLength(200);
    }
}
