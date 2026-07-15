using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Application.Users.Validators;

/// <summary>
/// Validation de forme uniquement. Les règles de sécurité "seul un Administrateur peut
/// attribuer le rôle Administrateur / une permission financière" (cahier des charges §6.4) ne
/// sont pas encore applicables : aucune authentification n'existe (différée après le Lot 0,
/// voir docs/IMPLEMENTATION_STATUS.md), donc aucun appelant authentifié à vérifier. Une
/// application partielle de cette règle serait une fausse protection — pire que son absence.
/// </summary>
public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator(
        IReadRepository<Resource> resourceRepository,
        IReadRepository<Role> roleRepository,
        IReadRepository<Permission> permissionRepository)
    {
        RuleFor(x => x.Nom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prenom).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Identifiant).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Telephone).MaximumLength(30);
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => id is null || await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.RoleId)
            .MustAsync(async (id, ct) => await roleRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("Le rôle applicatif indiqué n'existe pas.");

        RuleForEach(x => x.PermissionIds)
            .MustAsync(async (id, ct) => await permissionRepository.Query().AnyAsync(p => p.Id == id, ct))
            .WithMessage("Une des permissions indiquées n'existe pas.");
    }
}
