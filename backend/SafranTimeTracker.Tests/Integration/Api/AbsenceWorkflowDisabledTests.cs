using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Domain.Absences;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Classe dédiée avec sa propre base SQLite (IClassFixture) : modifie Settings.ActivationValidationAbsences,
/// un singleton partagé — isolée pour ne pas contaminer TimeAndCapacityTests.
/// </summary>
public class AbsenceWorkflowDisabledTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140";

    [Fact]
    public async Task Submit_WithValidationWorkflowDisabled_AutoValidatesImmediately()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(DemoCurrentUserProvider.DemoUserHeaderName, BernardIdentifiant);

        var settings = await client.GetFromJsonAsync<SettingsDto>("/api/v1/settings");
        var disableResponse = await client.PutAsJsonAsync("/api/v1/settings", new SettingsUpdateRequest
        {
            HeuresParJour = settings!.HeuresParJour,
            JoursOuvresParSemaine = settings.JoursOuvresParSemaine,
            PaysParDefaut = settings.PaysParDefaut,
            DeviseParDefaut = settings.DeviseParDefaut,
            SeuilSurcharge = settings.SeuilSurcharge,
            SeuilSousCharge = settings.SeuilSousCharge,
            SeuilAlerteBudget = settings.SeuilAlerteBudget,
            SeuilAlerteCommande = settings.SeuilAlerteCommande,
            DelaiModificationTempsJours = settings.DelaiModificationTempsJours,
            ActivationValidationAbsences = false,
            AutorisationSaisieSansValorisation = settings.AutorisationSaisieSansValorisation
        });
        disableResponse.EnsureSuccessStatusCode();

        var resources = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        var bernardId = resources!.Items.First(r => r.Nom == "BERNARD").Id;

        var created = await client.PostAsJsonAsync("/api/v1/absences", new AbsenceCreateRequest
        {
            ResourceId = bernardId, Type = AbsenceType.Conge, DateDebut = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5),
            DateFin = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5)
        });
        var dto = await created.Content.ReadFromJsonAsync<AbsenceDto>();

        var submitted = await client.PostAsync($"/api/v1/absences/{dto!.Id}/submit", null);
        var submittedDto = await submitted.Content.ReadFromJsonAsync<AbsenceDto>();

        submittedDto!.Statut.Should().Be(AbsenceStatus.Valide); // §23.3 : workflow désactivé -> validation immédiate
        submittedDto.ValideParIdentifiant.Should().Be(BernardIdentifiant);
    }
}
