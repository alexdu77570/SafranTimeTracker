using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Financial.Validators;

public class FinancialCalculationRequestValidator : AbstractValidator<FinancialCalculationRequest>
{
    public FinancialCalculationRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.HeuresSaisies).GreaterThan(0).WithMessage("Les heures saisies doivent être strictement positives.");
    }
}
