using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Organisation;

namespace SafranTimeTracker.Application.Organisation.Validators;

public class CostCenterCreateRequestValidator : AbstractValidator<CostCenterCreateRequest>
{
    public CostCenterCreateRequestValidator(
        IReadRepository<Department> departmentRepository, IReadRepository<Service> serviceRepository)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DepartmentId)
            .MustAsync(async (id, ct) => id is null || await departmentRepository.Query().AnyAsync(d => d.Id == id, ct))
            .WithMessage("Le département indiqué n'existe pas.");

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => id is null || await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");
    }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class CostCenterUpdateRequestValidator : AbstractValidator<CostCenterUpdateRequest>
{
    public CostCenterUpdateRequestValidator(
        IReadRepository<Department> departmentRepository, IReadRepository<Service> serviceRepository)
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(100);

        RuleFor(x => x.DepartmentId)
            .MustAsync(async (id, ct) => id is null || await departmentRepository.Query().AnyAsync(d => d.Id == id, ct))
            .WithMessage("Le département indiqué n'existe pas.");

        RuleFor(x => x.ServiceId)
            .MustAsync(async (id, ct) => id is null || await serviceRepository.Query().AnyAsync(s => s.Id == id, ct))
            .WithMessage("Le service indiqué n'existe pas.");
    }
}
