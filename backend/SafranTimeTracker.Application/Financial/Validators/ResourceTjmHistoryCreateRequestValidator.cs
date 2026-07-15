using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Financial.Validators;

public class ResourceTjmHistoryCreateRequestValidator : AbstractValidator<ResourceTjmHistoryCreateRequest>
{
    public ResourceTjmHistoryCreateRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.DailyRate).GreaterThan(0).WithMessage("Le TJM doit être strictement positif (cahier des charges §11.3).");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Reason).MaximumLength(200);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class ResourceTjmHistoryUpdateRequestValidator : AbstractValidator<ResourceTjmHistoryUpdateRequest>
{
    public ResourceTjmHistoryUpdateRequestValidator()
    {
        RuleFor(x => x.DailyRate).GreaterThan(0).WithMessage("Le TJM doit être strictement positif (cahier des charges §11.3).");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200)
            .WithMessage("Le motif de correction est obligatoire (cahier des charges §11.3).");
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
