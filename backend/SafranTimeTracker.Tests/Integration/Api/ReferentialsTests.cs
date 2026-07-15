using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Applications;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Vérifie que chaque référentiel du Lot 1 est réellement exposé et fonctionnel (liste seedée,
/// création valide, rejet d'une création invalide) — pas de test métier complexe ici, le Lot 1
/// ne contient pas de logique de calcul (voir docs/CONVENTIONS.md §8 pour la priorisation des
/// tests, qui s'applique pleinement à partir du Lot 2).
/// </summary>
public class ReferentialsTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // Les assertions ci-dessous vérifient la présence des données semées via Contain plutôt que
    // TotalCount exact : la fixture est partagée entre toutes les méthodes de test de cette classe
    // (IClassFixture), et certaines créent des lignes supplémentaires (POST) sans garantie d'ordre
    // d'exécution — asserter un total exact serait fragile.

    [Fact]
    public async Task GetDepartments_ReturnsSeededDsi()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<DepartmentDto>>("/api/v1/departments");

        result.Should().NotBeNull();
        result!.Items.Should().Contain(d => d.Code == "DSI");
    }

    [Fact]
    public async Task GetServices_ReturnsSeededServices()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ServiceDto>>("/api/v1/services");

        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task GetUsers_ReturnsSeededUsers()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users");

        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(13);
        result.Items.Should().Contain(u => u.Identifiant == "s636140");
    }

    [Fact]
    public async Task GetResources_ReturnsSeededResources()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources");

        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(13);
    }

    [Fact]
    public async Task GetApplications_ReturnsSeededApplications()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ApplicationReferenceDto>>("/api/v1/applications");

        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetCompanies_ReturnsSeededSafranCompany()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies");

        result.Should().NotBeNull();
        result!.Items.Should().Contain(c => c.Code == "SAFRAN");
    }

    [Fact]
    public async Task GetOrders_ReturnsSeededDemoOrderWithAuthorizedResources()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<OrderDto>>("/api/v1/orders");

        result.Should().NotBeNull();
        var order = result!.Items.Should().ContainSingle(o => o.Reference == "CMD-2026-0001").Subject;
        order.AuthorizedResourceIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSettings_ReturnsDefaultHeuresParJour()
    {
        var dto = await _client.GetFromJsonAsync<SettingsDto>("/api/v1/settings");

        dto.Should().NotBeNull();
        dto!.HeuresParJour.Should().Be(7.75m);
    }

    [Fact]
    public async Task PostDepartment_WithValidData_Returns201AndPersists()
    {
        var request = new DepartmentCreateRequest { Code = $"T{Guid.NewGuid():N}"[..8], Nom = "Département de test" };

        var response = await _client.PostAsJsonAsync("/api/v1/departments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<DepartmentDto>();
        created!.Code.Should().Be(request.Code);

        var fetched = await _client.GetFromJsonAsync<DepartmentDto>($"/api/v1/departments/{created.Id}");
        fetched!.Nom.Should().Be("Département de test");
    }

    [Fact]
    public async Task PostDepartment_WithEmptyCode_Returns400()
    {
        var request = new DepartmentCreateRequest { Code = "", Nom = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/departments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostService_WithUnknownDepartmentId_Returns400()
    {
        var request = new ServiceCreateRequest { Code = "ZZZ", Nom = "Service fantôme", DepartmentId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/v1/services", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostApplication_WithInvalidCriticite_Returns400()
    {
        var services = await _client.GetFromJsonAsync<PagedResult<ServiceDto>>("/api/v1/services");
        var serviceId = services!.Items.First().Id;

        var request = new ApplicationReferenceCreateRequest
        {
            Code = "ZZZ",
            Nom = "Application invalide",
            ServiceId = serviceId,
            Criticite = (ApplicationCriticality)999
        };

        var response = await _client.PostAsJsonAsync("/api/v1/applications", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDepartmentById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/departments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
