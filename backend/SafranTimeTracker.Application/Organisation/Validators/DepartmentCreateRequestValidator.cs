using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Organisation.Validators;

public class DepartmentCreateRequestValidator : AbstractValidator<DepartmentCreateRequest>
{
    public DepartmentCreateRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ResponsableId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource responsable indiquée n'existe pas.");
    }
}
