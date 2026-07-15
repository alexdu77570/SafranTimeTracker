using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Application.Reporting.Services;

/// <summary>
/// Résout une granularité de période (§21.1 "période") en bornes explicites, à partir d'une date
/// de référence. Fonction pure, testable sans base de données (CLAUDE.md §14) — seul
/// <c>ReportingService</c> l'invoque, même principe que <c>ProjectPlanningCalculator</c>.
/// </summary>
public static class ReportingPeriodResolver
{
    public static (DateOnly From, DateOnly To) Resolve(
        ReportingPeriodType periodType, DateOnly referenceDate, DateOnly? customFrom = null, DateOnly? customTo = null) =>
        periodType switch
        {
            ReportingPeriodType.Jour => (referenceDate, referenceDate),
            ReportingPeriodType.Semaine => ResolveWeek(referenceDate),
            ReportingPeriodType.Mois => (
                new DateOnly(referenceDate.Year, referenceDate.Month, 1),
                new DateOnly(referenceDate.Year, referenceDate.Month, DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month))),
            ReportingPeriodType.Annee => (new DateOnly(referenceDate.Year, 1, 1), new DateOnly(referenceDate.Year, 12, 31)),
            ReportingPeriodType.Personnalisee => (
                customFrom ?? throw new ArgumentException("Une période personnalisée exige une date de début.", nameof(customFrom)),
                customTo ?? throw new ArgumentException("Une période personnalisée exige une date de fin.", nameof(customTo))),
            _ => throw new ArgumentOutOfRangeException(nameof(periodType))
        };

    /// <summary>Semaine du lundi au dimanche (cohérent avec ProjectWeeklyPlan.WeekStartDate, Lot 4).</summary>
    private static (DateOnly From, DateOnly To) ResolveWeek(DateOnly referenceDate)
    {
        var daysSinceMonday = ((int)referenceDate.DayOfWeek + 6) % 7;
        var monday = referenceDate.AddDays(-daysSinceMonday);
        return (monday, monday.AddDays(6));
    }
}
