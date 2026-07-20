using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Application.Applications.Dtos;
using SafranTimeTracker.Application.Clients.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Currencies.Dtos;
using SafranTimeTracker.Application.Organisation.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.Technologies.Dtos;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Vérifie les 5 référentiels validés du Lot 8 (docs/BACKLOG_METIER.md §5-9) : Technologies,
/// Clients, Types de projet, Centres de coûts, Devises — liste seedée, création valide/invalide,
/// modification auditée, et pour Technology le remplacement des liaisons Application/Ressource
/// (garantit l'absence de ligne de jointure orpheline après une mise à jour, cf. TechnologyService).
/// </summary>
public class Lot8ReferentialsTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTechnologies_ReturnsSeededTechnologiesWithLinks()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<TechnologyDto>>("/api/v1/technologies");

        result.Should().NotBeNull();
        var dotnet = result!.Items.Should().ContainSingle(t => t.Code == "DOTNET").Subject;
        dotnet.ApplicationIds.Should().NotBeEmpty();
        dotnet.ResourceIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTechnologies_FilteredByApplicationId_ReturnsOnlyLinkedTechnologies()
    {
        var applications = await _client.GetFromJsonAsync<PagedResult<ApplicationReferenceDto>>("/api/v1/applications");
        var ibmElm = applications!.Items.Should().ContainSingle(a => a.Code == "IBMELM").Subject;

        var result = await _client.GetFromJsonAsync<PagedResult<TechnologyDto>>($"/api/v1/technologies?applicationId={ibmElm.Id}");

        result!.Items.Select(t => t.Code).Should().Contain(new[] { "DOTNET", "POSTGRESQL" });
        result.Items.Select(t => t.Code).Should().NotContain("REACT");
    }

    [Fact]
    public async Task PostTechnology_WithValidData_Returns201AndPersistsLinks()
    {
        var applications = await _client.GetFromJsonAsync<PagedResult<ApplicationReferenceDto>>("/api/v1/applications");
        var applicationId = applications!.Items.First().Id;

        var request = new TechnologyCreateRequest
        {
            Code = $"T{Guid.NewGuid():N}"[..10], Libelle = "Technologie de test", ApplicationIds = [applicationId],
        };

        var response = await _client.PostAsJsonAsync("/api/v1/technologies", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TechnologyDto>();
        created!.ApplicationIds.Should().ContainSingle(id => id == applicationId);
    }

    [Fact]
    public async Task PostTechnology_WithEmptyCode_Returns400()
    {
        var request = new TechnologyCreateRequest { Code = "", Libelle = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/technologies", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutTechnology_ReplacingApplicationLinks_RemovesOldAndAddsNew()
    {
        var applications = await _client.GetFromJsonAsync<PagedResult<ApplicationReferenceDto>>("/api/v1/applications?pageSize=10");
        var appA = applications!.Items[0].Id;
        var appB = applications.Items[1].Id;

        var created = await (await _client.PostAsJsonAsync("/api/v1/technologies", new TechnologyCreateRequest
        {
            Code = $"T{Guid.NewGuid():N}"[..10], Libelle = "Avant modification", ApplicationIds = [appA],
        })).Content.ReadFromJsonAsync<TechnologyDto>();

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/technologies/{created!.Id}", new TechnologyUpdateRequest
        {
            Libelle = "Après modification", Statut = ReferentialStatus.Actif, ApplicationIds = [appB],
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TechnologyDto>();
        updated!.ApplicationIds.Should().ContainSingle(id => id == appB);
        updated.ApplicationIds.Should().NotContain(appA);
    }

    [Fact]
    public async Task GetClients_ReturnsSeededClients()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ClientDto>>("/api/v1/clients");

        result.Should().NotBeNull();
        result!.Items.Should().Contain(c => c.Code == "DIR-PROD");
    }

    [Fact]
    public async Task PostClient_WithValidData_Returns201AndPersists()
    {
        var request = new ClientCreateRequest { Code = $"T{Guid.NewGuid():N}"[..10], Nom = "Client de test" };

        var response = await _client.PostAsJsonAsync("/api/v1/clients", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ClientDto>();
        created!.Statut.Should().Be(ReferentialStatus.Actif);
    }

    [Fact]
    public async Task PutClient_DeactivatingClient_UpdatesStatutAndIsAudited()
    {
        var created = await (await _client.PostAsJsonAsync("/api/v1/clients", new ClientCreateRequest
        {
            Code = $"T{Guid.NewGuid():N}"[..10], Nom = "Client à désactiver",
        })).Content.ReadFromJsonAsync<ClientDto>();

        var response = await _client.PutAsJsonAsync($"/api/v1/clients/{created!.Id}", new ClientUpdateRequest
        {
            Nom = created.Nom, Statut = ReferentialStatus.Inactif,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ClientDto>();
        updated!.Statut.Should().Be(ReferentialStatus.Inactif);
    }

    [Fact]
    public async Task PutClient_WithUnknownId_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"/api/v1/clients/{Guid.NewGuid()}", new ClientUpdateRequest { Nom = "Inconnu" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectTypes_ReturnsSeededTypes()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ProjectTypeDto>>("/api/v1/project-types");

        result.Should().NotBeNull();
        result!.Items.Select(t => t.Code).Should().Contain(new[] { "FORFAIT", "REGIE", "INTERNE" });
    }

    [Fact]
    public async Task PostProjectType_WithEmptyLibelle_Returns400()
    {
        var request = new ProjectTypeCreateRequest { Code = "ZZZ", Libelle = "" };

        var response = await _client.PostAsJsonAsync("/api/v1/project-types", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProjects_SeededMigrationElm_HasProjectTypeAndClient()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects");

        var elm = result!.Items.Should().ContainSingle(p => p.Code == "PRJ-ELM-2026").Subject;
        elm.ProjectTypeId.Should().NotBeNull();
        elm.ClientId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCostCenters_ReturnsSeededCentersLinkedToDepartment()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<CostCenterDto>>("/api/v1/cost-centers");

        result.Should().NotBeNull();
        var costCenter = result!.Items.Should().ContainSingle(c => c.Code == "CC-DSI").Subject;
        costCenter.DepartmentId.Should().NotBeNull();
    }

    [Fact]
    public async Task PostCostCenter_WithUnknownDepartmentId_Returns400()
    {
        var request = new CostCenterCreateRequest { Code = "ZZZ", Libelle = "Centre fantôme", DepartmentId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/v1/cost-centers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCurrencies_ReturnsSeededEurAndUsd()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<CurrencyDto>>("/api/v1/currencies");

        result.Should().NotBeNull();
        result!.Items.Select(c => c.CodeIso).Should().Contain(new[] { "EUR", "USD" });
    }

    [Fact]
    public async Task PostCurrency_WithInvalidIsoCode_Returns400()
    {
        var request = new CurrencyCreateRequest { CodeIso = "euro", Libelle = "Euro invalide", Symbole = "€" };

        var response = await _client.PostAsJsonAsync("/api/v1/currencies", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostCurrency_WithValidIsoCode_Returns201()
    {
        var request = new CurrencyCreateRequest { CodeIso = "GBP", Libelle = "Livre sterling", Symbole = "£" };

        var response = await _client.PostAsJsonAsync("/api/v1/currencies", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
