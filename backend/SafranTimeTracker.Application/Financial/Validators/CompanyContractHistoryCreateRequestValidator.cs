using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Application.Financial.Validators;

public class CompanyContractHistoryCreateRequestValidator : AbstractValidator<CompanyContractHistoryCreateRequest>
{
    public CompanyContractHistoryCreateRequestValidator(IReadRepository<Company> companyRepository)
    {
        RuleFor(x => x.CompanyId)
            .MustAsync(async (id, ct) => await companyRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("La société indiquée n'existe pas.");

        RuleFor(x => x.ContractDailyRate).GreaterThan(0).WithMessage("Le TJM du contrat doit être strictement positif (cahier des charges §12.4).");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("La devise doit être un code ISO 4217 à 3 lettres.");
        RuleFor(x => x.ContractNumber).MaximumLength(50);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class CompanyContractHistoryUpdateRequestValidator : AbstractValidator<CompanyContractHistoryUpdateRequest>
{
    public CompanyContractHistoryUpdateRequestValidator()
    {
        RuleFor(x => x.ContractDailyRate).GreaterThan(0).WithMessage("Le TJM du contrat doit être strictement positif (cahier des charges §12.4).");
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("La devise doit être un code ISO 4217 à 3 lettres.");
        RuleFor(x => x.ContractNumber).MaximumLength(50);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000)
            .WithMessage("Le motif de correction est obligatoire (cahier des charges §12.4).");
    }
}
