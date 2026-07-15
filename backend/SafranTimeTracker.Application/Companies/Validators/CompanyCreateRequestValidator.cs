using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Domain.Companies;

namespace SafranTimeTracker.Application.Companies.Validators;

public class CompanyCreateRequestValidator : AbstractValidator<CompanyCreateRequest>
{
    public CompanyCreateRequestValidator(IReadRepository<CompanyType> companyTypeRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ContactPrincipal).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmailContact).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Telephone).MaximumLength(30);
        RuleFor(x => x.Adresse).MaximumLength(500);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.CompanyTypeId)
            .MustAsync(async (id, ct) => await companyTypeRepository.Query().AnyAsync(t => t.Id == id, ct))
            .WithMessage("Le type de société indiqué n'existe pas.");
    }
}
