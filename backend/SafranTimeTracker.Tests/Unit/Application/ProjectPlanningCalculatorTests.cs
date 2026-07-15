using FluentAssertions;
using SafranTimeTracker.Application.Projects.Services;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Cahier des charges §29.5 : écarts et risques projet, testés sans base de données
/// (CLAUDE.md §14 "écarts projet").</summary>
public class ProjectPlanningCalculatorTests
{
    [Fact]
    public void CalculateChargeMetrics_WithoutAdjustedVersion_UsesInitialAsReference()
    {
        var result = ProjectPlanningCalculator.CalculateChargeMetrics(chargeInitiale: 100m, chargeAjustee: null, chargeConsommee: 40m);

        result.ChargeRestante.Should().Be(60m);
        result.EcartCharge.Should().Be(-60m); // réalisé (40) - prévu (100)
        result.DeriveCharge.Should().Be(0m); // pas de version ajustée
        result.AtterrissageCharge.Should().Be(100m); // 40 + 60
    }

    [Fact]
    public void CalculateChargeMetrics_WithAdjustedVersion_UsesAdjustedAsReference()
    {
        var result = ProjectPlanningCalculator.CalculateChargeMetrics(chargeInitiale: 100m, chargeAjustee: 130m, chargeConsommee: 40m);

        result.ChargeRestante.Should().Be(90m); // 130 - 40
        result.EcartCharge.Should().Be(-90m); // 40 - 130
        result.DeriveCharge.Should().Be(30m); // 130 - 100
        result.AtterrissageCharge.Should().Be(130m); // 40 + 90
    }

    [Fact]
    public void CalculateChargeMetrics_WhenConsumedExceedsPlanned_ReturnsPositiveEcart()
    {
        var result = ProjectPlanningCalculator.CalculateChargeMetrics(chargeInitiale: 100m, chargeAjustee: null, chargeConsommee: 120m);

        result.EcartCharge.Should().Be(20m); // dépassement
        result.ChargeRestante.Should().Be(-20m);
    }

    [Fact]
    public void CalculatePlanningRisk_WithNoAdjustedEndDate_ReturnsNoRiskAndZeroDrift()
    {
        var result = ProjectPlanningCalculator.CalculatePlanningRisk(new DateOnly(2024, 12, 31), null);

        result.DerivePlanningJours.Should().Be(0);
        result.RisquePlanning.Should().BeFalse();
    }

    [Fact]
    public void CalculatePlanningRisk_WithAdjustedEndDateLater_ReturnsRiskAndPositiveDrift()
    {
        var result = ProjectPlanningCalculator.CalculatePlanningRisk(new DateOnly(2024, 12, 31), new DateOnly(2025, 3, 31));

        result.DerivePlanningJours.Should().Be(90);
        result.RisquePlanning.Should().BeTrue();
    }

    [Fact]
    public void CalculatePlanningRisk_WithAdjustedEndDateEarlier_ReturnsNoRiskDespiteNegativeDrift()
    {
        // §29.5 : risque planning uniquement "si la date ajustée dépasse la date initiale".
        var result = ProjectPlanningCalculator.CalculatePlanningRisk(new DateOnly(2024, 12, 31), new DateOnly(2024, 11, 30));

        result.DerivePlanningJours.Should().Be(-31);
        result.RisquePlanning.Should().BeFalse();
    }

    [Theory]
    [InlineData(150, 100, true)]
    [InlineData(100, 100, false)]
    [InlineData(50, 100, false)]
    public void CalculateBudgetRisk_ComparesConsumedCostToInitialBudget(decimal coutReel, decimal budget, bool expectedRisk)
    {
        var result = ProjectPlanningCalculator.CalculateBudgetRisk(coutReel, budget);

        result.Should().Be(expectedRisk);
    }
}
