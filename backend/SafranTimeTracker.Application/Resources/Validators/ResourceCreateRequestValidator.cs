using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;
using DepartmentEntity = SafranTimeTracker.Domain.Organisation.Department;
using ServiceEntity = SafranTimeTracker.Domain.Organisation.Service;
using TeamEntity = SafranTimeTracker.Domain.Organisation.Team;

namespace SafranTimeTracker.Application.Resources.Validators;

public class ResourceCreateRequestValidator : AbstractValidator<ResourceCreateRequest>
{
    public ResourceCreateRequestValidator(
        IReadRepository<DepartmentEntity> departmentRepository,
        IReadRepository<ServiceEntity> serviceRepository,
        IReadRepository<TeamEntity> teamRepository,
        IReadRepository<Resource> resourceRepository,
        IReadRepository<Company> companyRepository,
        IReadRepository<Order> orderRepository,
        IReadRepository<OperationalRole> operationalRoleRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DailyCapacity).GreaterThan(0);
        RuleFor(x => x.WeeklyCapacity).GreaterThan(0);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.DepartmentId)
            .MustAsync(async (id, ct) => await departmentRepository.Query().AnyAsync(d => d.Id == id, ct))
            .WithMessage("Le département indiqué n'existe pas.");

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");

        RuleFor(x => x.TeamId)
            .MustAsync(async (id, ct) => id is null || await teamRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("L'équipe indiquée n'existe pas.");

        RuleFor(x => x.ResponsableHierarchiqueId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le responsable hiérarchique indiqué n'existe pas.");

        RuleFor(x => x.CompanyId)
            .MustAsync(async (id, ct) => id is null || await companyRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("La société indiquée n'existe pas.");

        RuleFor(x => x.DefaultOrderId)
            .MustAsync(async (id, ct) => id is null || await orderRepository.Query().AnyAsync(o => o.Id == id, ct))
            .WithMessage("La commande par défaut indiquée n'existe pas.");

        RuleForEach(x => x.OperationalRoleIds)
            .MustAsync(async (id, ct) => await operationalRoleRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Un des rôles opérationnels indiqués n'existe pas.");
    }
}
