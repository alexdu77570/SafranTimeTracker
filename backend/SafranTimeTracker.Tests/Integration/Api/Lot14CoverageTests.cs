using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Sous-lot 14.1 (rapport d'audit du Lot 14) : couvre les 5 domaines backend constatés à 0 % de
/// couverture, masqués jusqu'ici par des seuils de couverture agrégés par projet — HolidayCalendar,
/// Team, MilestoneType, ResourceCapacityPeriod, ResourceCompanyAssignment. Un fichier dédié plutôt
/// qu'une extension de Lot8ReferentialsTests.cs : ces 5 domaines proviennent de lots différents
/// (Lot 1, 2, 3, 4), pas du Lot 8.
/// </summary>
public class Lot14CoverageTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // Administrateur, FINANCIAL_DATA_VIEW (Lot1Seed)

    private readonly HttpClient _client = factory.CreateClient();

    private HttpClient CreateClient(string? identifiant = null)
    {
        var client = factory.CreateClient();
        if (identifiant is not null)
        {
            client.DefaultRequestHeaders.Add(DemoCurrentUserProvider.DemoUserHeaderName, identifiant);
        }
        return client;
    }

    // ---------- HolidayCalendar ----------

    [Fact]
    public async Task GetHolidayCalendar_ReturnsSeededHolidays()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<HolidayCalendarDto>>("/api/v1/holiday-calendar?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHolidayCalendar_FilteredByYear_ReturnsOnlyThatYear()
    {
        var all = await _client.GetFromJsonAsync<PagedResult<HolidayCalendarDto>>("/api/v1/holiday-calendar?pageSize=100");
        var year = all!.Items.First().Date.Year;

        var result = await _client.GetFromJsonAsync<PagedResult<HolidayCalendarDto>>($"/api/v1/holiday-calendar?year={year}&pageSize=100");

        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(h => h.Date.Year == year);
    }

    [Fact]
    public async Task PostHolidayCalendar_WithValidData_Returns201()
    {
        var request = new HolidayCalendarCreateRequest
        {
            Date = new DateOnly(2027, 5, 1), Libelle = "Jour férié de test", Pays = "FR"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/holiday-calendar", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<HolidayCalendarDto>();
        created!.Libelle.Should().Be("Jour férié de test");
    }

    [Fact]
    public async Task PostHolidayCalendar_WithEmptyPays_Returns400()
    {
        var request = new HolidayCalendarCreateRequest { Date = new DateOnly(2027, 5, 2), Libelle = "Sans pays", Pays = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/holiday-calendar", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- Team ----------

    [Fact]
    public async Task GetTeams_ReturnsSeededTeams()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<TeamDto>>("/api/v1/teams?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTeams_FilteredByServiceId_ReturnsOnlyThatServiceTeams()
    {
        var teams = await _client.GetFromJsonAsync<PagedResult<TeamDto>>("/api/v1/teams?pageSize=100");
        var serviceId = teams!.Items.First().ServiceId;

        var result = await _client.GetFromJsonAsync<PagedResult<TeamDto>>($"/api/v1/teams?serviceId={serviceId}&pageSize=100");

        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(t => t.ServiceId == serviceId);
    }

    [Fact]
    public async Task PostTeam_WithValidServiceId_Returns201()
    {
        var services = await _client.GetFromJsonAsync<PagedResult<ServiceDto>>("/api/v1/services?pageSize=100");
        var serviceId = services!.Items.First().Id;
        var code = $"T{Guid.NewGuid():N}"[..10];

        var response = await _client.PostAsJsonAsync("/api/v1/teams", new TeamCreateRequest
        {
            Code = code, Nom = "Équipe de test", ServiceId = serviceId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TeamDto>();
        created!.ServiceId.Should().Be(serviceId);
    }

    [Fact]
    public async Task PostTeam_WithUnknownServiceId_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/teams", new TeamCreateRequest
        {
            Code = "ZZZ", Nom = "Équipe fantôme", ServiceId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- MilestoneType ----------

    [Fact]
    public async Task GetMilestoneTypes_ReturnsSeededTypes()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<MilestoneTypeDto>>("/api/v1/milestone-types?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostMilestoneType_WithValidData_Returns201()
    {
        var request = new MilestoneTypeCreateRequest { Code = $"T{Guid.NewGuid():N}"[..10], Libelle = "Type de jalon de test" };

        var response = await _client.PostAsJsonAsync("/api/v1/milestone-types", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostMilestoneType_WithEmptyCode_Returns400()
    {
        var request = new MilestoneTypeCreateRequest { Code = "", Libelle = "Sans code" };

        var response = await _client.PostAsJsonAsync("/api/v1/milestone-types", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- ResourceCapacityPeriod ----------

    [Fact]
    public async Task GetResourceCapacityPeriods_ReturnsSeededPeriods()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ResourceCapacityPeriodDto>>("/api/v1/resource-capacity-periods?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetResourceCapacityPeriods_FilteredByResourceId_ReturnsOnlyThatResource()
    {
        var periods = await _client.GetFromJsonAsync<PagedResult<ResourceCapacityPeriodDto>>("/api/v1/resource-capacity-periods?pageSize=100");
        var resourceId = periods!.Items.First().ResourceId;

        var result = await _client.GetFromJsonAsync<PagedResult<ResourceCapacityPeriodDto>>(
            $"/api/v1/resource-capacity-periods?resourceId={resourceId}&pageSize=100");

        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.ResourceId == resourceId);
    }

    [Fact]
    public async Task PostResourceCapacityPeriod_WithValidResourceId_Returns201()
    {
        var resources = await _client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        var resourceId = resources!.Items.First().Id;

        var response = await _client.PostAsJsonAsync("/api/v1/resource-capacity-periods", new ResourceCapacityPeriodCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2027, 1, 1), DailyCapacity = 4m, WeeklyCapacity = 20m,
            Reason = "Temps partiel de test"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResourceCapacityPeriodDto>();
        created!.DailyCapacity.Should().Be(4m);
    }

    [Fact]
    public async Task PostResourceCapacityPeriod_WithUnknownResourceId_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/resource-capacity-periods", new ResourceCapacityPeriodCreateRequest
        {
            ResourceId = Guid.NewGuid(), StartDate = new DateOnly(2027, 1, 1), DailyCapacity = 8m, WeeklyCapacity = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostResourceCapacityPeriod_WithDailyCapacityAboveTwentyFour_Returns400()
    {
        var resources = await _client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        var resourceId = resources!.Items.First().Id;

        var response = await _client.PostAsJsonAsync("/api/v1/resource-capacity-periods", new ResourceCapacityPeriodCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2027, 1, 1), DailyCapacity = 30m, WeeklyCapacity = 40m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- ResourceCompanyAssignment (gardé par FINANCIAL_DATA_VIEW) ----------

    [Fact]
    public async Task GetResourceCompanyAssignments_WithoutFinancialDataView_Returns403()
    {
        var client = CreateClient(); // aucune identité

        var response = await client.GetAsync("/api/v1/resource-company-assignments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetResourceCompanyAssignments_WithFinancialDataView_ReturnsSeededAssignments()
    {
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<ResourceCompanyAssignmentDto>>("/api/v1/resource-company-assignments?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostResourceCompanyAssignment_WithoutFinancialDataView_Returns403()
    {
        var client = CreateClient(); // aucune identité

        var response = await client.PostAsJsonAsync("/api/v1/resource-company-assignments", new ResourceCompanyAssignmentCreateRequest
        {
            ResourceId = Guid.NewGuid(), CompanyId = Guid.NewGuid(), StartDate = new DateOnly(2027, 1, 1), AssignmentType = "Interne"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostResourceCompanyAssignment_WithValidData_Returns201()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await CreateFreshResourceAsync(client);
        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=100");

        var response = await client.PostAsJsonAsync("/api/v1/resource-company-assignments", new ResourceCompanyAssignmentCreateRequest
        {
            ResourceId = resourceId, CompanyId = companies!.Items.First().Id,
            StartDate = new DateOnly(2027, 1, 1), AssignmentType = "Interne"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PutResourceCompanyAssignment_WithoutComment_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await CreateFreshResourceAsync(client);
        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=100");
        var created = await (await client.PostAsJsonAsync("/api/v1/resource-company-assignments", new ResourceCompanyAssignmentCreateRequest
        {
            ResourceId = resourceId, CompanyId = companies!.Items.First().Id,
            StartDate = new DateOnly(2027, 2, 1), AssignmentType = "Interne"
        })).Content.ReadFromJsonAsync<ResourceCompanyAssignmentDto>();

        var response = await client.PutAsJsonAsync($"/api/v1/resource-company-assignments/{created!.Id}", new ResourceCompanyAssignmentUpdateRequest
        {
            StartDate = created.StartDate, AssignmentType = "Interne", Comment = "", Status = ReferentialStatus.Actif
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutResourceCompanyAssignment_WithComment_Returns200AndPersists()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await CreateFreshResourceAsync(client);
        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=100");
        var created = await (await client.PostAsJsonAsync("/api/v1/resource-company-assignments", new ResourceCompanyAssignmentCreateRequest
        {
            ResourceId = resourceId, CompanyId = companies!.Items.First().Id,
            StartDate = new DateOnly(2027, 3, 1), AssignmentType = "Interne"
        })).Content.ReadFromJsonAsync<ResourceCompanyAssignmentDto>();

        var response = await client.PutAsJsonAsync($"/api/v1/resource-company-assignments/{created!.Id}", new ResourceCompanyAssignmentUpdateRequest
        {
            StartDate = created.StartDate, EndDate = new DateOnly(2027, 12, 31), AssignmentType = "Externe",
            Comment = "Correction de test", Status = ReferentialStatus.Actif
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ResourceCompanyAssignmentDto>();
        updated!.AssignmentType.Should().Be("Externe");
    }

    /// <summary>Une ressource fraîchement créée n'a aucun rattachement société préexistant,
    /// contrairement aux ressources seedées (rattachement ouvert depuis le Lot 1) — évite un 409
    /// de chevauchement (`ResourceCompanyAssignmentService.EnsureNoOverlapAsync`) sans dépendre
    /// d'une ressource seedée précise, cohérente avec CLAUDE.md §14 (créer sa propre entité plutôt
    /// que muter/réutiliser une entité seedée partagée par d'autres tests).</summary>
    private static async Task<Guid> CreateFreshResourceAsync(HttpClient client)
    {
        var departments = await client.GetFromJsonAsync<PagedResult<DepartmentDto>>("/api/v1/departments?pageSize=1");
        var services = await client.GetFromJsonAsync<PagedResult<ServiceDto>>("/api/v1/services?pageSize=1");
        var identifiant = $"T{Guid.NewGuid():N}"[..10];

        var response = await client.PostAsJsonAsync("/api/v1/resources", new ResourceCreateRequest
        {
            Nom = "TEST", Prenom = identifiant, DepartmentId = departments!.Items.First().Id,
            ServiceId = services!.Items.First().Id, DailyCapacity = 8m, WeeklyCapacity = 40m
        });
        var created = await response.Content.ReadFromJsonAsync<ResourceDto>();
        return created!.Id;
    }
}
