using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Application.Auth.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Users.Dtos;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 13 : session simulée (création/révocation par cookie, sans en-tête X-Demo-User) et
/// modèle RBAC (permissions effectives = rôle ∪ octrois individuels − retraits individuels). Les
/// tests des Lots 0-12 continuent de poser l'en-tête directement (façade de compatibilité
/// Development/Test, <c>Authentication:AllowDirectDemoHeader</c>) — non dupliqué ici.
/// </summary>
public class Lot13AuthenticationRbacTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // Administrateur (Lot1Seed)

    [Fact]
    public async Task CreateSession_WithValidIdentifiant_ReturnsSessionDetailsAndSetsCookie()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/sessions", new AuthSessionRequest { Identifiant = BernardIdentifiant });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<AuthSessionDto>();
        dto!.Identifiant.Should().Be(BernardIdentifiant);
        dto.IsPersistent.Should().BeFalse();
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith("stt_session=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CreateSession_WithUnknownIdentifiant_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/sessions", new AuthSessionRequest { Identifiant = "inconnu" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSession_ThenAccessFinancialEndpoint_UsesSessionIdentityWithoutHeader()
    {
        // HandleCookies (par défaut sur WebApplicationFactory.CreateClient) rejoue le cookie posé par
        // la première requête sur la seconde, sans jamais définir l'en-tête X-Demo-User.
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/sessions", new AuthSessionRequest { Identifiant = BernardIdentifiant });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.GetAsync("/api/v1/company-contracts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeSession_ThenAccessFinancialEndpoint_Returns403()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/auth/sessions", new AuthSessionRequest { Identifiant = BernardIdentifiant });

        var logout = await client.DeleteAsync("/api/v1/auth/sessions");
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.GetAsync("/api/v1/company-contracts");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUser_ForAdministrateurRole_EffectivePermissionCodesIncludesAllSeededPermissions()
    {
        var client = CreateHeaderClient(BernardIdentifiant);
        var bernardId = await GetUserIdAsync(client, "BERNARD");

        var dto = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{bernardId}");

        dto!.EffectivePermissionCodes.Should().BeEquivalentTo(
        [
            "FINANCIAL_DATA_VIEW", "TIME_ENTRY_CORRECTION", "USER_ADMINISTRATION",
            "TIME_ENTRY_RECALCULATION", "IMPORT_EXECUTE", "AUDIT_VIEW", "ORDER_RECEIPT_OVERRIDE"
        ]);
    }

    [Fact]
    public async Task GetUser_ForIngenieurRoleWithoutIndividualGrant_EffectivePermissionCodesIsEmpty()
    {
        var client = CreateHeaderClient(BernardIdentifiant);
        var created = await CreateTestUserAsync(client, roleSourceNom: "MISHRA");

        var dto = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{created.Id}");

        dto!.EffectivePermissionCodes.Should().BeEmpty();
    }

    [Fact]
    public async Task GrantPermission_ForRoleThatAlreadyGrantsIt_ReturnsConflict()
    {
        var client = CreateHeaderClient(BernardIdentifiant);
        var administrateur = await PromoteFreshUserToAdministrateurAsync(client);

        var response = await client.PostAsync($"/api/v1/users/{administrateur.Id}/permissions/AUDIT_VIEW", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RevokePermission_WhenRoleStillGrantsIt_MaterializesOverride_ThenRegrantRestoresAccess()
    {
        var client = CreateHeaderClient(BernardIdentifiant);
        var administrateur = await PromoteFreshUserToAdministrateurAsync(client);

        // Le rôle ADMINISTRATEUR accorde AUDIT_VIEW (Lot13Seed) sans qu'aucune ligne individuelle
        // n'existe pour ce nouvel utilisateur : le retrait doit matérialiser un retrait explicite.
        var revoked = await client.DeleteAsync($"/api/v1/users/{administrateur.Id}/permissions/AUDIT_VIEW");
        revoked.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterRevoke = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{administrateur.Id}");
        afterRevoke!.EffectivePermissionCodes.Should().NotContain("AUDIT_VIEW");

        var regranted = await client.PostAsync($"/api/v1/users/{administrateur.Id}/permissions/AUDIT_VIEW", null);
        regranted.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterRegrant = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{administrateur.Id}");
        afterRegrant!.EffectivePermissionCodes.Should().Contain("AUDIT_VIEW");
    }

    private HttpClient CreateHeaderClient(string identifiant)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(SafranTimeTracker.Api.Security.DemoCurrentUserProvider.DemoUserHeaderName, identifiant);
        return client;
    }

    private static async Task<Guid> GetUserIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users?pageSize=100");
        return result!.Items.First(u => u.Nom == nom).Id;
    }

    private static async Task<Guid> GetUserRoleIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users?pageSize=100");
        return result!.Items.First(u => u.Nom == nom).RoleId;
    }

    /// <summary>Crée un utilisateur de test dédié plutôt que de muter un utilisateur seedé réutilisé
    /// par d'autres tests de la même classe (CLAUDE.md §14, convention actée au Lot 5).</summary>
    private static async Task<UserDto> CreateTestUserAsync(HttpClient client, string roleSourceNom)
    {
        var roleId = await GetUserRoleIdAsync(client, roleSourceNom);
        var identifiant = $"test-{Guid.NewGuid():N}"[..20];

        var response = await client.PostAsJsonAsync("/api/v1/users", new UserCreateRequest
        {
            Nom = "TEST", Prenom = "Utilisateur", Identifiant = identifiant, Email = $"{identifiant}@example.com",
            DateArrivee = DateOnly.FromDateTime(DateTime.UtcNow), RoleId = roleId
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<UserDto>())!;
    }

    private static async Task<UserDto> PromoteFreshUserToAdministrateurAsync(HttpClient client)
    {
        var created = await CreateTestUserAsync(client, roleSourceNom: "MISHRA");
        var administrateurRoleId = await GetUserRoleIdAsync(client, "BERNARD");

        var promoted = await client.PutAsJsonAsync($"/api/v1/users/{created.Id}/role", new RoleChangeRequest
        {
            RoleId = administrateurRoleId, Motif = "Promotion de démonstration (test Lot 13)."
        });
        promoted.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await promoted.Content.ReadFromJsonAsync<UserDto>())!;
    }
}
