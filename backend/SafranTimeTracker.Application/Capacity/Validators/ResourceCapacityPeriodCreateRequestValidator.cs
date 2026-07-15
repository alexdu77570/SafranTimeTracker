using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Capacity.Validators;

public class ResourceCapacityPeriodCreateRequestValidator : AbstractValidator<ResourceCapacityPeriodCreateRequest>
{
    public ResourceCapacityPeriodCreateRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.DailyCapacity).GreaterThan(0).LessThanOrEqualTo(24);
        RuleFor(x => x.WeeklyCapacity).GreaterThan(0).LessThanOrEqualTo(168);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Reason).MaximumLength(200);
    }
}
