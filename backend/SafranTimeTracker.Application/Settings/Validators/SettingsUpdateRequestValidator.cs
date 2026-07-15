using FluentValidation;
using SafranTimeTracker.Application.Settings.Dtos;

namespace SafranTimeTracker.Application.Settings.Validators;

public class SettingsUpdateRequestValidator : AbstractValidator<SettingsUpdateRequest>
{
    public SettingsUpdateRequestValidator()
    {
        RuleFor(x => x.HeuresParJour).GreaterThan(0).LessThanOrEqualTo(24);
        RuleFor(x => x.JoursOuvresParSemaine).InclusiveBetween(1, 7);
        RuleFor(x => x.PaysParDefaut).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DeviseParDefaut).NotEmpty().Length(3);
        RuleFor(x => x.SeuilSurcharge).InclusiveBetween(0, 1000).When(x => x.SeuilSurcharge is not null);
        RuleFor(x => x.SeuilSousCharge).InclusiveBetween(0, 1000).When(x => x.SeuilSousCharge is not null);
        RuleFor(x => x.SeuilAlerteBudget).InclusiveBetween(0, 100).When(x => x.SeuilAlerteBudget is not null);
        RuleFor(x => x.SeuilAlerteCommande).InclusiveBetween(0, 100).When(x => x.SeuilAlerteCommande is not null);
        RuleFor(x => x.DelaiModificationTempsJours).GreaterThanOrEqualTo(0);
    }
}
