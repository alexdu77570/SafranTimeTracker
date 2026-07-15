using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Absences.Validators;

public class AbsenceCreateRequestValidator : AbstractValidator<AbsenceCreateRequest>
{
    public AbsenceCreateRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.DateFin).GreaterThanOrEqualTo(x => x.DateDebut)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Commentaire).MaximumLength(1000);
    }
}

public class AbsenceDecisionRequestValidator : AbstractValidator<AbsenceDecisionRequest>
{
    public AbsenceDecisionRequestValidator()
    {
        RuleFor(x => x.Commentaire).MaximumLength(1000);
    }
}
