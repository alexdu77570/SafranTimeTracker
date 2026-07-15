using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 2 de bout en bout via l'API réelle (SQLite, migrations et seed appliqués) :
/// garde de permission FINANCIAL_DATA_VIEW basée sur ICurrentUser/DemoCurrentUserProvider,
/// résolution TJM/contrat/société à la date demandée (§11, §12, §20), chevauchement en 409.
/// </summary>
public class FinancialModelTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // FINANCIAL_DATA_VIEW (Lot1Seed)
    private const string LegrandIdentifiant = "flegrand"; // FINANCIAL_DATA_VIEW (Lot1Seed)
    private const string DurandIdentifiant = "cdurand"; // utilisateur actif sans permission financière

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
    public async Task GetResourceTjmHistory_WithoutDemoHeader_Returns403()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/resource-tjm-history");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetResourceTjmHistory_WithUserWithoutFinancialPermission_Returns403()
    {
        var client = CreateClient(DurandIdentifiant);

        var response = await client.GetAsync("/api/v1/resource-tjm-history");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetResourceTjmHistory_WithFinancialPermission_ReturnsSeededData()
    {
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<ResourceTjmHistoryDto>>("/api/v1/resource-tjm-history?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().Contain(h => h.DailyRate == 650.00m);
    }

    [Fact]
    public async Task GetCompanyContracts_WithFinancialPermission_ReturnsSeededExternalContract()
    {
        var client = CreateClient(LegrandIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<CompanyContractHistoryDto>>("/api/v1/company-contracts?pageSize=100");

        result.Should().NotBeNull();
        result!.Items.Should().Contain(c => c.ContractDailyRate == 750.00m && c.Currency == "EUR");
    }

    [Fact]
    public async Task PreviewCalculation_ForInternalCompanyResource_ReturnsRealCostOnlyContractualNotApplicable()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "BERNARD");

        var response = await client.PostAsJsonAsync("/api/v1/financial-calculations/preview", new FinancialCalculationRequest
        {
            ResourceId = resourceId,
            Date = new DateOnly(2024, 6, 15),
            HeuresSaisies = 7.75m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FinancialCalculationResultDto>();
        result!.ValuationStatus.Should().Be(FinancialValuationStatus.Complete);
        result.CoutReel.Should().Be(650.00m);
        result.CoutContractuel.Should().BeNull();
        result.Differentiel.Should().BeNull();
    }

    [Fact]
    public async Task PreviewCalculation_ForExternalCompanyResourceWithContract_ReturnsFullBreakdown()
    {
        var client = CreateClient(LegrandIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "LEGRAND");

        var response = await client.PostAsJsonAsync("/api/v1/financial-calculations/preview", new FinancialCalculationRequest
        {
            ResourceId = resourceId,
            Date = new DateOnly(2024, 6, 15),
            HeuresSaisies = 7.75m
        });

        var result = await response.Content.ReadFromJsonAsync<FinancialCalculationResultDto>();
        result!.ValuationStatus.Should().Be(FinancialValuationStatus.Complete);
        result.CoutReel.Should().Be(700.00m);
        result.CoutContractuel.Should().Be(750.00m);
        result.Differentiel.Should().Be(50.00m); // §20.4 : coutContrat - coutReel
    }

    [Fact]
    public async Task PreviewCalculation_ForResourceWithoutTjmHistory_ReturnsIncompleteValuation()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "MISHRA");

        var response = await client.PostAsJsonAsync("/api/v1/financial-calculations/preview", new FinancialCalculationRequest
        {
            ResourceId = resourceId,
            Date = new DateOnly(2024, 6, 15),
            HeuresSaisies = 7.75m
        });

        var result = await response.Content.ReadFromJsonAsync<FinancialCalculationResultDto>();
        result!.ValuationStatus.Should().Be(FinancialValuationStatus.Incomplete);
        result.CoutReel.Should().BeNull();
    }

    [Fact]
    public async Task PreviewCalculation_ForResourceWithTjmChangeOverTime_UsesRateApplicableAtRequestedDate()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "GEORGES");

        var beforeResponse = await client.PostAsJsonAsync("/api/v1/financial-calculations/preview", new FinancialCalculationRequest
        {
            ResourceId = resourceId, Date = new DateOnly(2024, 6, 15), HeuresSaisies = 7.75m
        });
        var afterResponse = await client.PostAsJsonAsync("/api/v1/financial-calculations/preview", new FinancialCalculationRequest
        {
            ResourceId = resourceId, Date = new DateOnly(2025, 6, 15), HeuresSaisies = 7.75m
        });

        var before = await beforeResponse.Content.ReadFromJsonAsync<FinancialCalculationResultDto>();
        var after = await afterResponse.Content.ReadFromJsonAsync<FinancialCalculationResultDto>();

        before!.DailyRatePersonne.Should().Be(600.00m);
        after!.DailyRatePersonne.Should().Be(620.00m);
    }

    [Fact]
    public async Task PostResourceTjmHistory_WithOverlappingPeriod_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "BERNARD"); // période ouverte depuis 2024-01-01 (seed)

        var response = await client.PostAsJsonAsync("/api/v1/resource-tjm-history", new ResourceTjmHistoryCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2024, 6, 1), DailyRate = 800.00m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostResourceTjmHistory_WithNonPositiveDailyRate_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "MISHRA");

        var response = await client.PostAsJsonAsync("/api/v1/resource-tjm-history", new ResourceTjmHistoryCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2026, 1, 1), DailyRate = 0
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateThenClosePeriod_AllowsNewNonOverlappingPeriodAfterwards()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "PATEL"); // sans historique TJM préexistant (seed)

        var created = await client.PostAsJsonAsync("/api/v1/resource-tjm-history", new ResourceTjmHistoryCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2024, 1, 1), DailyRate = 500.00m
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdDto = await created.Content.ReadFromJsonAsync<ResourceTjmHistoryDto>();

        var closed = await client.PutAsJsonAsync($"/api/v1/resource-tjm-history/{createdDto!.Id}", new ResourceTjmHistoryUpdateRequest
        {
            StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31),
            DailyRate = 500.00m, Reason = "Clôture de test"
        });
        closed.StatusCode.Should().Be(HttpStatusCode.OK);

        var next = await client.PostAsJsonAsync("/api/v1/resource-tjm-history", new ResourceTjmHistoryCreateRequest
        {
            ResourceId = resourceId, StartDate = new DateOnly(2025, 1, 1), DailyRate = 520.00m
        });
        next.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static async Task<Guid> GetResourceIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        return result!.Items.First(r => r.Nom == nom).Id;
    }
}
