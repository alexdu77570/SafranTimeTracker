using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Resources;
using DepartmentEntity = SafranTimeTracker.Domain.Organisation.Department;

namespace SafranTimeTracker.Application.Organisation.Validators;

public class ServiceCreateRequestValidator : AbstractValidator<ServiceCreateRequest>
{
    public ServiceCreateRequestValidator(IReadRepository<DepartmentEntity> departmentRepository, IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.DepartmentId)
            .MustAsync(async (id, ct) => await departmentRepository.Query().AnyAsync(d => d.Id == id, ct))
            .WithMessage("Le département indiqué n'existe pas.");

        RuleFor(x => x.ResponsableId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource responsable indiquée n'existe pas.");
    }
}
