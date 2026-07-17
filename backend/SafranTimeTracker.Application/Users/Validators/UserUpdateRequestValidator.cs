using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users.Validators;

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator(IReadRepository<Resource> resourceRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Telephone).MaximumLength(30);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");
    }
}

public class RoleChangeRequestValidator : AbstractValidator<RoleChangeRequest>
{
    public RoleChangeRequestValidator(IReadRepository<Role> roleRepository)
    {
        RuleFor(x => x.RoleId)
            .MustAsync(async (id, ct) => await roleRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le rôle applicatif indiqué n'existe pas.");

        RuleFor(x => x.Motif).MaximumLength(500);
    }
}
