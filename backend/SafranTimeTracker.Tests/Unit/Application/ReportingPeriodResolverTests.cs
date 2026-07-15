using FluentAssertions;
using SafranTimeTracker.Application.Reporting.Services;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Cahier des charges §21.1 "période" : résolution testée sans base de données
/// (CLAUDE.md §14).</summary>
public class ReportingPeriodResolverTests
{
    [Fact]
    public void Resolve_Jour_ReturnsSameDayForFromAndTo()
    {
        var result = ReportingPeriodResolver.Resolve(ReportingPeriodType.Jour, new DateOnly(2026, 3, 12));

        result.From.Should().Be(new DateOnly(2026, 3, 12));
        result.To.Should().Be(new DateOnly(2026, 3, 12));
    }

    [Fact]
    public void Resolve_Semaine_FromMidWeekDate_ReturnsMondayToSunday()
    {
        // 2026-03-12 est un jeudi.
        var result = ReportingPeriodResolver.Resolve(ReportingPeriodType.Semaine, new DateOnly(2026, 3, 12));

        result.From.Should().Be(new DateOnly(2026, 3, 9)); // lundi
        result.To.Should().Be(new DateOnly(2026, 3, 15)); // dimanche
    }

    [Fact]
    public void Resolve_Semaine_FromSunday_StillReturnsPrecedingMonday()
    {
        var result = ReportingPeriodResolver.Resolve(ReportingPeriodType.Semaine, new DateOnly(2026, 3, 15));

        result.From.Should().Be(new DateOnly(2026, 3, 9));
        result.To.Should().Be(new DateOnly(2026, 3, 15));
    }

    [Fact]
    public void Resolve_Mois_ReturnsFirstAndLastDayOfMonth()
    {
        var result = ReportingPeriodResolver.Resolve(ReportingPeriodType.Mois, new DateOnly(2026, 2, 17));

        result.From.Should().Be(new DateOnly(2026, 2, 1));
        result.To.Should().Be(new DateOnly(2026, 2, 28)); // 2026 n'est pas bissextile
    }

    [Fact]
    public void Resolve_Annee_ReturnsFirstAndLastDayOfYear()
    {
        var result = ReportingPeriodResolver.Resolve(ReportingPeriodType.Annee, new DateOnly(2026, 6, 30));

        result.From.Should().Be(new DateOnly(2026, 1, 1));
        result.To.Should().Be(new DateOnly(2026, 12, 31));
    }

    [Fact]
    public void Resolve_Personnalisee_WithBounds_ReturnsThoseBounds()
    {
        var result = ReportingPeriodResolver.Resolve(
            ReportingPeriodType.Personnalisee, new DateOnly(2026, 1, 1), new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));

        result.From.Should().Be(new DateOnly(2026, 5, 1));
        result.To.Should().Be(new DateOnly(2026, 5, 31));
    }

    [Fact]
    public void Resolve_Personnalisee_WithoutBounds_Throws()
    {
        var act = () => ReportingPeriodResolver.Resolve(ReportingPeriodType.Personnalisee, new DateOnly(2026, 1, 1));

        act.Should().Throw<ArgumentException>();
    }
}
