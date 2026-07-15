using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Resources;
using ServiceEntity = SafranTimeTracker.Domain.Organisation.Service;

namespace SafranTimeTracker.Application.Organisation.Validators;

public class TeamCreateRequestValidator : AbstractValidator<TeamCreateRequest>
{
    public TeamCreateRequestValidator(IReadRepository<ServiceEntity> serviceRepository, IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");

        RuleFor(x => x.ResponsableFonctionnelId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource responsable fonctionnel indiquée n'existe pas.");
    }
}
