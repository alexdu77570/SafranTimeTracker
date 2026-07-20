using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Clients;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Projects.Validators;

public class ProjectCreateRequestValidator : AbstractValidator<ProjectCreateRequest>
{
    public ProjectCreateRequestValidator(
        IReadRepository<ApplicationReference> applicationRepository,
        IReadRepository<Resource> resourceRepository,
        IReadRepository<Department> departmentRepository,
        IReadRepository<Service> serviceRepository,
        IReadRepository<Team> teamRepository,
        IReadRepository<ProjectType> projectTypeRepository,
        IReadRepository<Client> clientRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DescriptionCourte).MaximumLength(500);
        RuleFor(x => x.Commentaire).MaximumLength(1000);
        RuleFor(x => x.BudgetInitial).GreaterThan(0).When(x => x.BudgetInitial is not null);
        RuleFor(x => x.DateFinPrevueInitiale).GreaterThanOrEqualTo(x => x.DateDebut)
            .WithMessage("La date de fin prévue initiale doit être postérieure ou égale à la date de début.");

        RuleFor(x => x.ApplicationId)
            .MustAsync(async (id, ct) => await applicationRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("L'application principale indiquée n'existe pas.");

        RuleFor(x => x.PiloteId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le pilote indiqué n'existe pas.");

        RuleFor(x => x.DepartmentId)
            .MustAsync(async (id, ct) => await departmentRepository.Query().AnyAsync(d => d.Id == id, ct))
            .WithMessage("Le département indiqué n'existe pas.");

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");

        RuleFor(x => x.TeamId)
            .MustAsync(async (id, ct) => id is null || await teamRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("L'équipe indiquée n'existe pas.");

        RuleFor(x => x.ProjectTypeId)
            .MustAsync(async (id, ct) => id is null || await projectTypeRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("Le type de projet indiqué n'existe pas.");

        RuleFor(x => x.ClientId)
            .MustAsync(async (id, ct) => id is null || await clientRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("Le client indiqué n'existe pas.");
    }
}

public class ProjectUpdateRequestValidator : AbstractValidator<ProjectUpdateRequest>
{
    public ProjectUpdateRequestValidator(
        IReadRepository<Resource> resourceRepository,
        IReadRepository<Team> teamRepository,
        IReadRepository<ProjectType> projectTypeRepository,
        IReadRepository<Client> clientRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DescriptionCourte).MaximumLength(500);
        RuleFor(x => x.Commentaire).MaximumLength(1000);
        RuleFor(x => x.BudgetInitial).GreaterThan(0).When(x => x.BudgetInitial is not null);

        RuleFor(x => x.PiloteId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le pilote indiqué n'existe pas.");

        RuleFor(x => x.TeamId)
            .MustAsync(async (id, ct) => id is null || await teamRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("L'équipe indiquée n'existe pas.");

        RuleFor(x => x.ProjectTypeId)
            .MustAsync(async (id, ct) => id is null || await projectTypeRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("Le type de projet indiqué n'existe pas.");

        RuleFor(x => x.ClientId)
            .MustAsync(async (id, ct) => id is null || await clientRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("Le client indiqué n'existe pas.");
    }
}
