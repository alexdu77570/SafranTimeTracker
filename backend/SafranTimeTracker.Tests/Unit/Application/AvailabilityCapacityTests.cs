using FluentAssertions;
using SafranTimeTracker.Application.Capacity.Services;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Cahier des charges §29.1 : capacité journalière applicable à une date, testée sans
/// base de données (fonction pure, CLAUDE.md §14 "calcul capacitaire").</summary>
public class AvailabilityCapacityTests
{
    private static Resource DefaultResource() => new() { DailyCapacity = 7.75m, WeeklyCapacity = 38.75m };

    [Fact]
    public void GetApplicableDailyCapacity_WithNoPeriodCoveringDate_ReturnsResourceDefault()
    {
        var resource = DefaultResource();

        var result = AvailabilityService.GetApplicableDailyCapacity(resource, [], new DateOnly(2024, 6, 10));

        result.Should().Be(7.75m);
    }

    [Fact]
    public void GetApplicableDailyCapacity_WithPeriodCoveringDate_ReturnsPeriodCapacity()
    {
        var resource = DefaultResource();
        var periods = new List<ResourceCapacityPeriod>
        {
            new() { StartDate = new DateOnly(2024, 1, 1), EndDate = null, DailyCapacity = 4.00m, Status = ReferentialStatus.Actif }
        };

        var result = AvailabilityService.GetApplicableDailyCapacity(resource, periods, new DateOnly(2024, 6, 10));

        result.Should().Be(4.00m);
    }

    [Fact]
    public void GetApplicableDailyCapacity_WithDateOutsidePeriod_ReturnsResourceDefault()
    {
        var resource = DefaultResource();
        var periods = new List<ResourceCapacityPeriod>
        {
            new() { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 3, 31), DailyCapacity = 4.00m, Status = ReferentialStatus.Actif }
        };

        var result = AvailabilityService.GetApplicableDailyCapacity(resource, periods, new DateOnly(2024, 6, 10));

        result.Should().Be(7.75m);
    }

    [Fact]
    public void GetApplicableDailyCapacity_WithMultiplePeriods_ReturnsMostRecentApplicable()
    {
        var resource = DefaultResource();
        var periods = new List<ResourceCapacityPeriod>
        {
            new() { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 6, 30), DailyCapacity = 4.00m, Status = ReferentialStatus.Actif },
            new() { StartDate = new DateOnly(2024, 7, 1), EndDate = null, DailyCapacity = 6.00m, Status = ReferentialStatus.Actif }
        };

        var result = AvailabilityService.GetApplicableDailyCapacity(resource, periods, new DateOnly(2024, 8, 1));

        result.Should().Be(6.00m);
    }
}
