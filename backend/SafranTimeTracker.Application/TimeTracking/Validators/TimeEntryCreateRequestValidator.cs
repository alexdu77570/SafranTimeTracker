using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Application.TimeTracking.Validators;

public class TimeEntryCreateRequestValidator : AbstractValidator<TimeEntryCreateRequest>
{
    public TimeEntryCreateRequestValidator(
        IReadRepository<Resource> resourceRepository,
        IReadRepository<ActivityType> activityTypeRepository,
        IReadRepository<Order> orderRepository,
        IReadRepository<Project> projectRepository)
    {
        RuleFor(x => x.ResourceId)
            .MustAsync(async (id, ct) => await resourceRepository.Query().AnyAsync(r => r.Id == id, ct))
            .WithMessage("La ressource indiquée n'existe pas.");

        RuleFor(x => x.ActivityTypeId)
            .MustAsync(async (id, ct) => await activityTypeRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("Le type d'activité indiqué n'existe pas.");

        RuleFor(x => x.OrderId)
            .MustAsync(async (id, ct) => id is null || await orderRepository.Query().AnyAsync(o => o.Id == id, ct))
            .WithMessage("La commande indiquée n'existe pas.");

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => id is null || await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .WithMessage("Le projet indiqué n'existe pas.");

        RuleFor(x => x.DureeHeures).GreaterThan(0).LessThanOrEqualTo(24)
            .WithMessage("La durée doit être strictement positive et ne peut excéder 24 heures.");
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.Reference)
            .CustomAsync(async (reference, context, cancellationToken) =>
                await ReferenceValidators.ValidateAsync(activityTypeRepository, context.InstanceToValidate.ActivityTypeId, reference, context, cancellationToken));
    }
}

public class TimeEntryUpdateRequestValidator : AbstractValidator<TimeEntryUpdateRequest>
{
    public TimeEntryUpdateRequestValidator(
        IReadRepository<ActivityType> activityTypeRepository, IReadRepository<Order> orderRepository, IReadRepository<Project> projectRepository)
    {
        RuleFor(x => x.ActivityTypeId)
            .MustAsync(async (id, ct) => await activityTypeRepository.Query().AnyAsync(a => a.Id == id, ct))
            .WithMessage("Le type d'activité indiqué n'existe pas.");

        RuleFor(x => x.OrderId)
            .MustAsync(async (id, ct) => id is null || await orderRepository.Query().AnyAsync(o => o.Id == id, ct))
            .WithMessage("La commande indiquée n'existe pas.");

        RuleFor(x => x.ProjectId)
            .MustAsync(async (id, ct) => id is null || await projectRepository.Query().AnyAsync(p => p.Id == id, ct))
            .WithMessage("Le projet indiqué n'existe pas.");

        RuleFor(x => x.DureeHeures).GreaterThan(0).LessThanOrEqualTo(24)
            .WithMessage("La durée doit être strictement positive et ne peut excéder 24 heures.");
        RuleFor(x => x.Commentaire).MaximumLength(1000);

        RuleFor(x => x.Reference)
            .CustomAsync(async (reference, context, cancellationToken) =>
                await ReferenceValidators.ValidateAsync(activityTypeRepository, context.InstanceToValidate.ActivityTypeId, reference, context, cancellationToken));
    }
}

/// <summary>
/// Validation de la référence entièrement pilotée par les métadonnées d'ActivityType
/// (ReferenceRequired/ReferenceFormatRegex/ReferenceExample, cahier des charges §19.3) : aucun
/// type d'activité codé en dur ici, partagé par Create et Update.
/// </summary>
internal static class ReferenceValidators
{
    public static async Task ValidateAsync<T>(
        IReadRepository<ActivityType> activityTypeRepository, Guid activityTypeId, string? reference,
        ValidationContext<T> context, CancellationToken cancellationToken)
    {
        var activityType = await activityTypeRepository.GetByIdAsync(activityTypeId, cancellationToken);
        if (activityType is null)
        {
            return; // Erreur déjà remontée par la règle d'existence sur ActivityTypeId.
        }

        if (activityType.ReferenceRequired && string.IsNullOrWhiteSpace(reference))
        {
            context.AddFailure(
                $"La référence est obligatoire pour le type d'activité '{activityType.Libelle}' (exemple : {activityType.ReferenceExample}).");
            return;
        }

        if (!string.IsNullOrWhiteSpace(reference) && !string.IsNullOrEmpty(activityType.ReferenceFormatRegex)
            && !Regex.IsMatch(reference, activityType.ReferenceFormatRegex))
        {
            context.AddFailure(
                $"La référence ne respecte pas le format attendu pour '{activityType.Libelle}' (exemple : {activityType.ReferenceExample}).");
        }
    }
}
