using FluentValidation;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Validators;

/// <summary>Sous-lot 14.1 (rapport d'audit du Lot 14, constat BE-7) : sans ce validateur,
/// <c>periodType=Personnalisee</c> sans <c>customFrom</c>/<c>customTo</c> faisait fuir une
/// <see cref="ArgumentException"/> brute depuis <see cref="ReportingPeriodResolver"/>, non
/// interceptée par <c>BusinessConflictExceptionHandler</c> — un 500 là où la convention du projet
/// (CLAUDE.md §12) impose un 400 sur une erreur de validation.</summary>
public class ReportingFilterQueryValidator : AbstractValidator<ReportingFilterQuery>
{
    public ReportingFilterQueryValidator()
    {
        RuleFor(x => x.CustomFrom)
            .NotNull()
            .When(x => x.PeriodType == ReportingPeriodType.Personnalisee)
            .WithMessage("Une période personnalisée exige une date de début (customFrom).");

        RuleFor(x => x.CustomTo)
            .NotNull()
            .When(x => x.PeriodType == ReportingPeriodType.Personnalisee)
            .WithMessage("Une période personnalisée exige une date de fin (customTo).");

        RuleFor(x => x.CustomTo)
            .GreaterThanOrEqualTo(x => x.CustomFrom!.Value)
            .When(x => x.PeriodType == ReportingPeriodType.Personnalisee && x.CustomFrom is not null && x.CustomTo is not null)
            .WithMessage("La date de fin d'une période personnalisée doit être postérieure ou égale à la date de début.");
    }
}
