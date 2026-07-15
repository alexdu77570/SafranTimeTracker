using FluentAssertions;
using SafranTimeTracker.Application.Common;

namespace SafranTimeTracker.Tests.Unit.Application;

public class DateRangeOverlapTests
{
    [Fact]
    public void Overlaps_WithAdjacentPeriodsNotSharingADay_ReturnsFalse()
    {
        var result = DateRangeOverlap.Overlaps(
            new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 30),
            new DateOnly(2024, 7, 1), null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithPeriodsSharingASingleDay_ReturnsTrue()
    {
        var result = DateRangeOverlap.Overlaps(
            new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 30),
            new DateOnly(2024, 6, 30), null);

        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithTwoOpenEndedPeriods_ReturnsTrue()
    {
        var result = DateRangeOverlap.Overlaps(
            new DateOnly(2024, 1, 1), null,
            new DateOnly(2025, 1, 1), null);

        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithNewPeriodEntirelyBeforeExisting_ReturnsFalse()
    {
        var result = DateRangeOverlap.Overlaps(
            new DateOnly(2024, 6, 1), null,
            new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31));

        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithNewPeriodFullyContainedInExisting_ReturnsTrue()
    {
        var result = DateRangeOverlap.Overlaps(
            new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31),
            new DateOnly(2024, 3, 1), new DateOnly(2024, 3, 31));

        result.Should().BeTrue();
    }
}
