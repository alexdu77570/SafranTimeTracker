using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.Orders.Validators;

public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator(
        IReadRepository<Company> companyRepository, IReadRepository<Resource> resourceRepository, IReadRepository<Project> projectRepository)
    {
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BudgetFinancierInitial).GreaterThan(0);
        RuleFor(x => x.BudgetJoursInitial).GreaterThan(0).When(x => x.BudgetJoursInitial is not null);
        RuleFor(x => x.DateFinInitiale).GreaterThanOrEqualTo(x => x.DateDebut)
            .WithMessage("La date de fin initiale doit être postérieure ou égale à la date de début.");
        RuleFor(x => x.SeuilAlerte).InclusiveBetween(0, 100).When(x => x.SeuilAlerte is not null);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.CompanyId)
            .MustAsync(async (id, ct) => await companyRepository.Query().AnyAsync(c => c.Id == id, ct))
            .WithMessage("La société indiquée n'existe pas.");

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .When(x => x.ProjectId is not null)
            .WithMessage("Le projet lié indiqué n'existe pas.");

        RuleForEach(x => x.AuthorizedResourceIds)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Une des ressources autorisées indiquées n'existe pas.");
    }
}

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator(IReadRepository<Resource> resourceRepository, IReadRepository<Project> projectRepository)
    {
        RuleFor(x => x.Libelle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SeuilAlerte).InclusiveBetween(0, 100).When(x => x.SeuilAlerte is not null);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .When(x => x.ProjectId is not null)
            .WithMessage("Le projet lié indiqué n'existe pas.");

        RuleForEach(x => x.AuthorizedResourceIds)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Une des ressources autorisées indiquées n'existe pas.");
    }
}

public class OrderReopenRequestValidator : AbstractValidator<OrderReopenRequest>
{
    public OrderReopenRequestValidator()
    {
        RuleFor(x => x.Motif).NotEmpty().MaximumLength(500);
    }
}
