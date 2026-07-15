using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Resources;
using ServiceEntity = SafranTimeTracker.Domain.Organisation.Service;
using TeamEntity = SafranTimeTracker.Domain.Organisation.Team;

namespace SafranTimeTracker.Application.Applications.Validators;

public class ApplicationReferenceCreateRequestValidator : AbstractValidator<ApplicationReferenceCreateRequest>
{
    public ApplicationReferenceCreateRequestValidator(
        IReadRepository<ServiceEntity> serviceRepository,
        IReadRepository<TeamEntity> teamRepository,
        IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Criticite).IsInEnum();
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");

        RuleFor(x => x.TeamId)
            .MustAsync(async (id, ct) => id is null || await teamRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("L'équipe indiquée n'existe pas.");

        RuleFor(x => x.ResponsableId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le responsable indiqué n'existe pas.");
    }
}
