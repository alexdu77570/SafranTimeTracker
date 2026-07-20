using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Technologies.Dtos;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Technologies.Validators;

public class TechnologyCreateRequestValidator : AbstractValidator<TechnologyCreateRequest>
{
    public TechnologyCreateRequestValidator(
        IReadRepository<ApplicationReference> applicationRepository, IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);

        RuleForEach(x => x.ApplicationIds)
            .MustAsync(async (id, ct) => await applicationRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("Une application indiquée n'existe pas.");

        RuleForEach(x => x.ResourceIds)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Une ressource indiquée n'existe pas.");
    }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class TechnologyUpdateRequestValidator : AbstractValidator<TechnologyUpdateRequest>
{
    public TechnologyUpdateRequestValidator(
        IReadRepository<ApplicationReference> applicationRepository, IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);

        RuleForEach(x => x.ApplicationIds)
            .MustAsync(async (id, ct) => await applicationRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("Une application indiquée n'existe pas.");

        RuleForEach(x => x.ResourceIds)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Une ressource indiquée n'existe pas.");
    }
}
