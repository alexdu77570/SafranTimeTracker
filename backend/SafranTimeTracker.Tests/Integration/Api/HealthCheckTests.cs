using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Vérifie que l'hôte ASP.NET Core démarre réellement (DI, EF Core, Serilog, Swagger) et expose
/// l'endpoint de santé attendu par les scripts de déploiement (CLAUDE.md §19, docs/ARCHITECTURE.md §7).
/// Aucune fonctionnalité métier n'est testée ici : c'est un test d'infrastructure du Lot 0.
/// </summary>
public class HealthCheckTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_ReturnsOkAndHealthy()
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"))
            .CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Be("Healthy");
    }

    [Fact]
    public async Task GetSwaggerDocument_ReturnsOkAndNoBusinessEndpoint()
    {
        var client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"))
            .CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("SAFRAN TIME TRACKER API");
    }
}
