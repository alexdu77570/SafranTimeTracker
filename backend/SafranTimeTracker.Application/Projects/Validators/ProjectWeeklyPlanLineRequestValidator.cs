using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Projects.Validators;

public class ProjectWeeklyPlanLineRequestValidator : AbstractValidator<ProjectWeeklyPlanLineRequest>
{
    public ProjectWeeklyPlanLineRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.ChargePlanifieeHeures).GreaterThanOrEqualTo(0).LessThanOrEqualTo(168)
            .WithMessage("La charge planifiée doit être comprise entre 0 et 168 heures (une semaine).");
    }
}
