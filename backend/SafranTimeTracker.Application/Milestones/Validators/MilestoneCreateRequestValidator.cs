using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Milestones.Validators;

public class MilestoneCreateRequestValidator : AbstractValidator<MilestoneCreateRequest>
{
    public MilestoneCreateRequestValidator(
        IReadRepository<MilestoneType> milestoneTypeRepository,
        IReadRepository<Project> projectRepository,
        IReadRepository<ApplicationReference> applicationRepository,
        IReadRepository<Resource> resourceRepository,
        IReadRepository<Milestone> milestoneRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.MilestoneTypeId)
            .MustAsync(async (id, ct) => await milestoneTypeRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("Le type de jalon indiqué n'existe pas.");

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .WithMessage("Le projet indiqué n'existe pas.");

        RuleFor(x => x.ApplicationId)
            .MustAsync(async (id, ct) => id is null || await applicationRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("L'application indiquée n'existe pas.");

        RuleFor(x => x.ResponsableId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le responsable indiqué n'existe pas.");

        RuleFor(x => x.DependsOnMilestoneId)
            .MustAsync(async (id, ct) => id is null || await milestoneRepository.Query().AnyAsync(m => m.Id == id, ct))
            .WithMessage("Le jalon dont dépend celui-ci n'existe pas.");
    }
}

public class MilestoneUpdateRequestValidator : AbstractValidator<MilestoneUpdateRequest>
{
    public MilestoneUpdateRequestValidator(IReadRepository<Resource> resourceRepository, IReadRepository<Milestone> milestoneRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ResponsableId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le responsable indiqué n'existe pas.");

        RuleFor(x => x.DependsOnMilestoneId)
            .MustAsync(async (id, ct) => id is null || await milestoneRepository.Query().AnyAsync(m => m.Id == id, ct))
            .WithMessage("Le jalon dont dépend celui-ci n'existe pas.");
    }
}
