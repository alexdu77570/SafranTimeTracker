using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Projects.Validators;

public class ProjectParticipantCreateRequestValidator : AbstractValidator<ProjectParticipantCreateRequest>
{
    public ProjectParticipantCreateRequestValidator(
        IReadRepository<Resource> resourceRepository,
        IReadRepository<OperationalRole> operationalRoleRepository,
        IReadRepository<Order> orderRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.OperationalRoleId)
            .MustAsync(async (id, ct) => id is null || await operationalRoleRepository.Query().AnyAsync(o => o.Id == id, ct))
            .WithMessage("Le rôle opérationnel indiqué n'existe pas.");

        RuleFor(x => x.DefaultOrderId)
            .MustAsync(async (id, ct) => id is null || await orderRepository.Query().AnyAsync(o => o.Id == id, ct))
            .WithMessage("La commande indiquée n'existe pas.");

        RuleFor(x => x.DateFin).GreaterThanOrEqualTo(x => x.DateDebut)
            .When(x => x.DateFin is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");

        RuleFor(x => x.CapacitePrevue).GreaterThan(0).When(x => x.CapacitePrevue is not null);
    }
}
