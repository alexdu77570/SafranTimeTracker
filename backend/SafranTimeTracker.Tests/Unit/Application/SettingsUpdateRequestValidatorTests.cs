using FluentAssertions;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Application.Settings.Validators;

namespace SafranTimeTracker.Tests.Unit.Application;

public class SettingsUpdateRequestValidatorTests
{
    private readonly SettingsUpdateRequestValidator _validator = new();

    private static SettingsUpdateRequest ValidRequest() => new()
    {
        HeuresParJour = 7.75m,
        JoursOuvresParSemaine = 5,
        PaysParDefaut = "France",
        DeviseParDefaut = "EUR",
        DelaiModificationTempsJours = 5
    };

    [Fact]
    public void Validate_WithDefaultValues_IsValid()
    {
        var result = _validator.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(-1)]
    public void Validate_WithOutOfRangeHeuresParJour_IsInvalid(decimal heuresParJour)
    {
        var request = ValidRequest();
        request.HeuresParJour = heuresParJour;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SettingsUpdateRequest.HeuresParJour));
    }

    [Fact]
    public void Validate_WithDeviseNotThreeLetters_IsInvalid()
    {
        var request = ValidRequest();
        request.DeviseParDefaut = "EURO";

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SettingsUpdateRequest.DeviseParDefaut));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void Validate_WithOutOfRangeJoursOuvresParSemaine_IsInvalid(int joursOuvres)
    {
        var request = ValidRequest();
        request.JoursOuvresParSemaine = joursOuvres;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SettingsUpdateRequest.JoursOuvresParSemaine));
    }
}
