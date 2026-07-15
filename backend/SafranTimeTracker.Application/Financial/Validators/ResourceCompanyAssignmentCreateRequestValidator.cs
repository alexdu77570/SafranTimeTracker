using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Financial.Validators;

public class ResourceCompanyAssignmentCreateRequestValidator : AbstractValidator<ResourceCompanyAssignmentCreateRequest>
{
    public ResourceCompanyAssignmentCreateRequestValidator(IReadRepository<Resource> resourceRepository, IReadRepository<Company> companyRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.CompanyId)
            .MustAsync(async (id, ct) => await companyRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("La société indiquée n'existe pas.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.AssignmentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class ResourceCompanyAssignmentUpdateRequestValidator : AbstractValidator<ResourceCompanyAssignmentUpdateRequest>
{
    public ResourceCompanyAssignmentUpdateRequestValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate is not null)
            .WithMessage("La date de fin doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.AssignmentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000)
            .WithMessage("Le motif de correction est obligatoire (cahier des charges §12.2).");
    }
}
