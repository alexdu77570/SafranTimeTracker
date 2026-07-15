using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Absences.Dtos;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Absences;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 3 de bout en bout via l'API réelle (SQLite, migrations et seed appliqués) :
/// valorisation figée d'une saisie de temps (§19.5), sous-objet financier omis sans
/// FINANCIAL_DATA_VIEW (CLAUDE.md §13), absence de recalcul rétroactif (§4.3, historisation TJM
/// dans le temps), compatibilité commande/société (§13.4), blocages ressource inactive/période
/// close (§19.4), validation de référence pilotée par ActivityType (§19.3), workflow d'absence
/// (§23.3), calculs de capacité et charge RUN/hors RUN (§29).
/// </summary>
public class TimeAndCapacityTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // FINANCIAL_DATA_VIEW (Lot1Seed)

    private HttpClient CreateClient(string? identifiant = null)
    {
        var client = factory.CreateClient();
        if (identifiant is not null)
        {
            client.DefaultRequestHeaders.Add(DemoCurrentUserProvider.DemoUserHeaderName, identifiant);
        }
        return client;
    }

    [Fact]
    public async Task GetTimeEntry_ForInternalCompanyResource_ReturnsRealCostOnly()
    {
        var client = CreateClient(BernardIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "BERNARD", "RUN");

        var dto = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entryId}");

        dto!.FinancialSnapshot.Should().NotBeNull();
        dto.FinancialSnapshot!.CalculationStatus.Should().Be(FinancialValuationStatus.Complete);
        dto.FinancialSnapshot.CoutReelCalcule.Should().Be(650.00m);
        dto.FinancialSnapshot.CoutContratCalcule.Should().BeNull();
        dto.FinancialSnapshot.DifferentielCalcule.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeEntry_ForExternalCompanyResourceWithContract_ReturnsFullBreakdown()
    {
        var client = CreateClient(BernardIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "LEGRAND", "PROJET");

        var dto = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entryId}");

        dto!.FinancialSnapshot!.CoutReelCalcule.Should().Be(700.00m);
        dto.FinancialSnapshot.CoutContratCalcule.Should().Be(750.00m);
        dto.FinancialSnapshot.DifferentielCalcule.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetTimeEntry_ForResourceWithoutTjm_ReturnsIncompleteSnapshot()
    {
        var client = CreateClient(BernardIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "MISHRA", "FORMATION");

        var dto = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entryId}");

        dto!.FinancialSnapshot!.CalculationStatus.Should().Be(FinancialValuationStatus.Incomplete);
        dto.FinancialSnapshot.CoutReelCalcule.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeEntry_WithoutFinancialPermission_OmitsFinancialSnapshot()
    {
        var client = CreateClient(); // pas d'en-tête -> pas de FINANCIAL_DATA_VIEW
        var entryId = await GetTimeEntryIdAsync(CreateClient(BernardIdentifiant), "BERNARD", "RUN");

        var dto = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entryId}");

        dto!.FinancialSnapshot.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeEntries_ForGeorgesAcrossTjmChange_KeepEachEntryOwnHistoricalRate()
    {
        // Démontre l'absence de recalcul rétroactif (§4.3) : la saisie 2024 garde le TJM 2024
        // (600) alors même que le TJM 2025 (620) existe déjà dans l'historique de la ressource.
        var client = CreateClient(BernardIdentifiant);
        var entry2024Id = await GetTimeEntryIdAsync(client, "GEORGES", "CHANGE", new DateOnly(2024, 3, 15));
        var entry2025Id = await GetTimeEntryIdAsync(client, "GEORGES", "CHANGE", new DateOnly(2025, 3, 15));

        var dto2024 = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entry2024Id}");
        var dto2025 = await client.GetFromJsonAsync<TimeEntryDto>($"/api/v1/time-entries/{entry2025Id}");

        dto2024!.FinancialSnapshot!.TjmPersonneSnapshot.Should().Be(600.00m);
        dto2025!.FinancialSnapshot!.TjmPersonneSnapshot.Should().Be(620.00m);
    }

    [Fact]
    public async Task PostTimeEntry_WithIncompatibleOrderCompany_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var legrandId = await GetResourceIdAsync(client, "LEGRAND"); // rattaché à une société externe
        var orderId = await GetOrderIdAsync(client, "CMD-2026-0001"); // société interne SAFRAN
        var activityTypeId = await GetActivityTypeIdAsync(client, "PROJET");

        var response = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = legrandId,
            ActivityTypeId = activityTypeId,
            OrderId = orderId,
            Date = Today(),
            DureeHeures = 7.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostTimeEntry_ForInactiveResource_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var inactiveResourceId = await GetResourceIdAsync(client, "RESSOURCE-INACTIVE");
        var activityTypeId = await GetActivityTypeIdAsync(client, "RUN");

        var response = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = inactiveResourceId,
            ActivityTypeId = activityTypeId,
            Date = Today(),
            DureeHeures = 7.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostTimeEntry_TooFarInThePast_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");
        var activityTypeId = await GetActivityTypeIdAsync(client, "RUN");

        var response = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = bernardId,
            ActivityTypeId = activityTypeId,
            Date = Today().AddDays(-30), // au-delà de Settings.DelaiModificationTempsJours (5, Lot1Seed)
            DureeHeures = 7.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostTimeEntry_WithMissingRequiredReference_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");
        var incidentTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");

        var response = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = bernardId,
            ActivityTypeId = incidentTypeId,
            Date = Today(),
            DureeHeures = 7.75m,
            Reference = null
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTimeEntry_WithInvalidReferenceFormat_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");
        var incidentTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");

        var response = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = bernardId,
            ActivityTypeId = incidentTypeId,
            Date = Today(),
            DureeHeures = 7.75m,
            Reference = "TICKET-123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateThenUpdateTimeEntry_RevaluatesWithNewDuration()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");
        var runTypeId = await GetActivityTypeIdAsync(client, "RUN");

        var created = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = bernardId, ActivityTypeId = runTypeId, Date = Today(), DureeHeures = 7.75m
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdDto = await created.Content.ReadFromJsonAsync<TimeEntryDto>();
        createdDto!.FinancialSnapshot!.CoutReelCalcule.Should().Be(650.00m); // 1 jour * 650

        var updated = await client.PutAsJsonAsync($"/api/v1/time-entries/{createdDto.Id}", new TimeEntryUpdateRequest
        {
            ActivityTypeId = runTypeId, Date = Today(), DureeHeures = 3.875m // demi-journée
        });
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDto = await updated.Content.ReadFromJsonAsync<TimeEntryDto>();
        updatedDto!.FinancialSnapshot!.CoutReelCalcule.Should().Be(325.00m); // 0.5 jour * 650
    }

    [Fact]
    public async Task AbsenceWorkflow_SubmitThenValidate_TransitionsCorrectly()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");

        var created = await client.PostAsJsonAsync("/api/v1/absences", new AbsenceCreateRequest
        {
            ResourceId = bernardId, Type = AbsenceType.Conge,
            DateDebut = Today().AddDays(10), DateFin = Today().AddDays(12), DemiJournee = false
        });
        var dto = await created.Content.ReadFromJsonAsync<AbsenceDto>();
        dto!.Statut.Should().Be(AbsenceStatus.Brouillon);

        var submitted = await client.PostAsync($"/api/v1/absences/{dto.Id}/submit", null);
        var submittedDto = await submitted.Content.ReadFromJsonAsync<AbsenceDto>();
        submittedDto!.Statut.Should().Be(AbsenceStatus.Soumis); // workflow activé par défaut (Lot1Seed)

        var validated = await client.PostAsync($"/api/v1/absences/{dto.Id}/validate", null);
        var validatedDto = await validated.Content.ReadFromJsonAsync<AbsenceDto>();
        validatedDto!.Statut.Should().Be(AbsenceStatus.Valide);
        validatedDto.ValideParIdentifiant.Should().Be(BernardIdentifiant);
    }

    [Fact]
    public async Task AbsenceWorkflow_ValidateWithoutSubmit_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");

        var created = await client.PostAsJsonAsync("/api/v1/absences", new AbsenceCreateRequest
        {
            ResourceId = bernardId, Type = AbsenceType.Rtt, DateDebut = Today().AddDays(20), DateFin = Today().AddDays(20)
        });
        var dto = await created.Content.ReadFromJsonAsync<AbsenceDto>();

        var response = await client.PostAsync($"/api/v1/absences/{dto!.Id}/validate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAvailability_ForResourceWithKnownHoliday_ReturnsExpectedCapacity()
    {
        // BERNARD, capacité par défaut 7,75h/jour (Lot1Seed), aucune période de capacité dédiée.
        // Semaine du 2024-04-29 au 2024-05-03 : 5 jours ouvrés dont le 1er mai (jour férié seedé).
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetResourceIdAsync(client, "BERNARD");

        var result = await client.GetFromJsonAsync<AvailabilityResultDto>(
            $"/api/v1/resources/{bernardId}/availability?startDate=2024-04-29&endDate=2024-05-03");

        result!.JoursOuvres.Should().Be(5);
        result.JoursFeries.Should().Be(1);
        result.CapaciteTheorique.Should().Be(38.75m); // 5 * 7.75
        result.CapaciteReelle.Should().Be(31.00m); // 38.75 - 7.75 (jour férié)
        result.TauxDisponibilite.Should().Be(80.00m);
    }

    [Fact]
    public async Task GetAvailability_ForLegrandInJune2024_ReturnsHorsRunWorkload()
    {
        // LEGRAND : une seule saisie seedée sur juin 2024, type "Projet" (hors RUN, §29.4).
        var client = CreateClient(BernardIdentifiant);
        var legrandId = await GetResourceIdAsync(client, "LEGRAND");

        var result = await client.GetFromJsonAsync<AvailabilityResultDto>(
            $"/api/v1/resources/{legrandId}/availability?startDate=2024-06-01&endDate=2024-06-30");

        result!.ChargeHorsRunHeures.Should().Be(7.75m);
        result.ChargeRunHeures.Should().Be(0m);
    }

    [Fact]
    public async Task GetAvailability_ForUnknownResource_Returns404()
    {
        var client = CreateClient(BernardIdentifiant);

        var response = await client.GetAsync($"/api/v1/resources/{Guid.NewGuid()}/availability?startDate=2024-01-01&endDate=2024-01-31");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<Guid> GetResourceIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        return result!.Items.First(r => r.Nom == nom).Id;
    }

    private static async Task<Guid> GetActivityTypeIdAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ActivityTypeDto>>("/api/v1/activity-types?pageSize=100");
        return result!.Items.First(a => a.Code == code).Id;
    }

    private static async Task<Guid> GetOrderIdAsync(HttpClient client, string reference)
    {
        var result = await client.GetFromJsonAsync<PagedResult<SafranTimeTracker.Application.Orders.Dtos.OrderDto>>("/api/v1/orders?pageSize=100");
        return result!.Items.First(o => o.Reference == reference).Id;
    }

    private static async Task<Guid> GetTimeEntryIdAsync(HttpClient client, string resourceNom, string activityTypeCode, DateOnly? date = null)
    {
        var resourceId = await GetResourceIdAsync(client, resourceNom);
        var result = await client.GetFromJsonAsync<PagedResult<TimeEntryDto>>($"/api/v1/time-entries?resourceId={resourceId}&pageSize=100");
        var activityTypeId = await GetActivityTypeIdAsync(client, activityTypeCode);
        var entry = date is null
            ? result!.Items.First(t => t.ActivityTypeId == activityTypeId)
            : result!.Items.First(t => t.ActivityTypeId == activityTypeId && t.Date == date);
        return entry.Id;
    }
}
