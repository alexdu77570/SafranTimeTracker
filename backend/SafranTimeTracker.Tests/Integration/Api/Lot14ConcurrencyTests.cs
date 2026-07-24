using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Sous-lot 14.2 (rapport d'audit du Lot 14, constat BE-1) : <c>Budget</c> et <c>Order</c> sont
/// nommés « entités sensibles » par CLAUDE.md §11 sans porter de jeton de concurrence jusqu'ici —
/// seuls ResourceTjmHistory/CompanyContractHistory en avaient un. Un conflit de concurrence ne peut
/// pas être déclenché par deux appels HTTP séquentiels seuls (le DTO n'expose pas le jeton au
/// client, par choix — CLAUDE.md §13, jamais une donnée technique interne) : le test charge
/// directement une seconde copie de l'entité via le <see cref="AppDbContext"/> du conteneur de test
/// (même technique que deux requêtes concurrentes réelles verraient chacune leur propre copie),
/// modifie l'entité via l'API (change le jeton en base), puis tente d'enregistrer la copie devenue
/// périmée — reproduisant exactement ce qu'EF Core intercepte en production.
/// </summary>
public class Lot14ConcurrencyTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // Administrateur, FINANCIAL_DATA_VIEW (Lot1Seed)

    private HttpClient CreateClient(string? identifiant = null)
    {
        var client = factory.CreateClient();
        if (identifiant is not null)
        {
            client.DefaultRequestHeaders.Add(SafranTimeTracker.Api.Security.DemoCurrentUserProvider.DemoUserHeaderName, identifiant);
        }
        return client;
    }

    [Fact]
    public async Task Budget_UpdatedConcurrently_ThrowsDbUpdateConcurrencyException()
    {
        var client = CreateClient(BernardIdentifiant);
        var budgetId = await CreateTestBudgetAsync(client);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var staleCopy = await dbContext.Set<Budget>().AsTracking().FirstAsync(b => b.Id == budgetId);

        // Une autre requête modifie le même budget entretemps (change ConcurrencyStamp en base).
        var response = await client.PutAsJsonAsync($"/api/v1/budgets/{budgetId}", new BudgetUpdateRequest
        {
            Name = "Budget modifié entretemps", AlertThreshold = 80m
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // La copie chargée avant cette modification tente d'enregistrer avec un jeton périmé :
        // EF Core a déjà capturé son ConcurrencyStamp d'alors comme valeur "originale" au chargement
        // (suivi activé par défaut sur DbContext.Set<T>()), donc SaveChangesAsync génère
        // WHERE id = @id AND concurrency_stamp = @valeurPérimée — 0 ligne affectée depuis la
        // modification HTTP ci-dessus, ce qui est précisément ce qu'EF Core traduit en
        // DbUpdateConcurrencyException.
        staleCopy.Name = "Écriture concurrente en conflit";

        var act = async () => await dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task Order_UpdatedConcurrently_ThrowsDbUpdateConcurrencyException()
    {
        var client = CreateClient(BernardIdentifiant);
        var (orderId, _) = await CreateTestOrderAsync(client);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var staleCopy = await dbContext.Set<Order>().AsTracking().FirstAsync(o => o.Id == orderId);

        var response = await client.PostAsync($"/api/v1/orders/{orderId}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        staleCopy.Commentaire = "Écriture concurrente en conflit";

        var act = async () => await dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    /// <summary>Note de conception délibérément non testée par un 3ᵉ scénario "deux appels HTTP
    /// séquentiels" : impossible à reproduire par construction. <c>BudgetUpdateRequest</c>/
    /// <c>OrderUpdateRequest</c> ne font jamais voyager le jeton de concurrence vers le client
    /// (CLAUDE.md §13 : jamais une donnée technique interne dans un DTO) — chaque appel HTTP de
    /// <c>UpdateAsync</c> recharge systématiquement l'entité via <c>repository.GetByIdAsync</c>
    /// juste avant de sauvegarder, donc deux requêtes HTTP strictement séquentielles ne peuvent
    /// jamais se percuter : la seconde voit toujours la valeur fraîche laissée par la première.
    /// La protection ne joue que pour deux écritures réellement concurrentes (déjà chargées avant
    /// que l'une des deux n'enregistre) — exactement le scénario reproduit ci-dessus au niveau
    /// DbContext, la seule façon fiable de le déclencher dans un test sans dépendre d'un minutage
    /// réel entre threads HTTP.</summary>
    private static async Task<Guid> CreateTestBudgetAsync(HttpClient client)
    {
        var (orderId, _) = await CreateTestOrderAsync(client);
        var response = await client.PostAsJsonAsync("/api/v1/budgets", new BudgetCreateRequest
        {
            Name = $"Budget de test {Guid.NewGuid():N}"[..30], OrderId = orderId,
            InitialAmount = 10000m, StartDate = new DateOnly(2027, 1, 1)
        });
        var created = await response.Content.ReadFromJsonAsync<BudgetDto>();
        return created!.Id;
    }

    private static async Task<(Guid OrderId, Guid CompanyId)> CreateTestOrderAsync(HttpClient client)
    {
        var companies = await client.GetFromJsonAsync<PagedResult<SafranTimeTracker.Application.Companies.Dtos.CompanyDto>>(
            "/api/v1/companies?pageSize=1");
        var companyId = companies!.Items.First().Id;

        var response = await client.PostAsJsonAsync("/api/v1/orders", new OrderCreateRequest
        {
            Reference = $"CMD-CONC-{Guid.NewGuid():N}"[..20], Libelle = "Commande de test (concurrence)",
            CompanyId = companyId, BudgetFinancierInitial = 10000m,
            DateDebut = new DateOnly(2027, 1, 1), DateFinInitiale = new DateOnly(2027, 12, 31)
        });
        var created = await response.Content.ReadFromJsonAsync<OrderDto>();
        return (created!.Id, companyId);
    }
}
