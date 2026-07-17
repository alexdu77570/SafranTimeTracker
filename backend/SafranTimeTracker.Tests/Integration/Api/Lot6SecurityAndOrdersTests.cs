using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Audit.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Application.Users.Dtos;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 6 côté sécurité/administration et commandes, via l'API réelle (SQLite, migrations
/// et seed appliqués) : administration utilisateur (§28.3 — modification, désactivation,
/// changement de rôle, octroi/retrait de permission, garde-fou dernier administrateur, CLAUDE.md
/// §17), suppression logique et recalcul explicite d'une saisie (§19.6, §28.3), réceptions
/// partielles d'une commande (règle métier validée Lot 6), modification d'une société (§28.3),
/// et consultation du journal d'audit (§28.1).
/// </summary>
public class Lot6SecurityAndOrdersTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // Administrateur + toutes les permissions Lot 6 (Lot6Seed)
    private const string LegrandIdentifiant = "flegrand"; // FINANCIAL_DATA_VIEW + TIME_ENTRY_CORRECTION uniquement

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
    public async Task UpdateUser_WithoutUserAdministration_Returns403()
    {
        var client = CreateClient(LegrandIdentifiant);
        var userId = await GetUserIdAsync(client, "MISHRA");

        var response = await client.PutAsJsonAsync($"/api/v1/users/{userId}", new UserUpdateRequest
        {
            Nom = "MISHRA", Prenom = "Reena", Email = "rmishra@example.com", DateArrivee = new DateOnly(2024, 1, 1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_WithUserAdministration_Returns200AndPersists()
    {
        var client = CreateClient(BernardIdentifiant);
        var userId = await GetUserIdAsync(client, "MISHRA");

        var response = await client.PutAsJsonAsync($"/api/v1/users/{userId}", new UserUpdateRequest
        {
            Nom = "MISHRA", Prenom = "Reena", Email = "rmishra-updated@example.com", DateArrivee = new DateOnly(2024, 1, 1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<UserDto>();
        dto!.Email.Should().Be("rmishra-updated@example.com");
    }

    [Fact]
    public async Task DeactivateAndReactivateUser_ChangesStatut()
    {
        var client = CreateClient(BernardIdentifiant);
        var created = await CreateTestUserAsync(client, roleSourceNom: "MISHRA");

        var deactivated = await client.PostAsync($"/api/v1/users/{created.Id}/deactivate", null);
        deactivated.StatusCode.Should().Be(HttpStatusCode.OK);
        var deactivatedDto = await deactivated.Content.ReadFromJsonAsync<UserDto>();
        deactivatedDto!.Statut.Should().Be(SafranTimeTracker.Domain.Common.ReferentialStatus.Inactif);

        var reactivated = await client.PostAsync($"/api/v1/users/{created.Id}/reactivate", null);
        reactivated.StatusCode.Should().Be(HttpStatusCode.OK);
        var reactivatedDto = await reactivated.Content.ReadFromJsonAsync<UserDto>();
        reactivatedDto!.Statut.Should().Be(SafranTimeTracker.Domain.Common.ReferentialStatus.Actif);
    }

    [Fact]
    public async Task ChangeRole_PromoteThenDemote_AuditsAdminGrantedAndRevoked()
    {
        var client = CreateClient(BernardIdentifiant);
        var created = await CreateTestUserAsync(client, roleSourceNom: "MISHRA");
        var ingenieurRoleId = created.RoleId;
        var administrateurRoleId = await GetUserRoleIdAsync(client, "BERNARD");

        var promoted = await client.PutAsJsonAsync($"/api/v1/users/{created.Id}/role", new RoleChangeRequest
        {
            RoleId = administrateurRoleId, Motif = "Promotion de démonstration (test)."
        });
        promoted.StatusCode.Should().Be(HttpStatusCode.OK);
        var promotedDto = await promoted.Content.ReadFromJsonAsync<UserDto>();
        promotedDto!.RoleId.Should().Be(administrateurRoleId);

        var demoted = await client.PutAsJsonAsync($"/api/v1/users/{created.Id}/role", new RoleChangeRequest
        {
            RoleId = ingenieurRoleId, Motif = "Retrait de démonstration (test)."
        });
        demoted.StatusCode.Should().Be(HttpStatusCode.OK);

        var auditLogs = await client.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"/api/v1/audit-logs?entityType=User&pageSize=200");
        auditLogs!.Items.Should().Contain(a => a.Action == "ADMIN_GRANTED" && a.EntityId == created.Id);
        auditLogs.Items.Should().Contain(a => a.Action == "ADMIN_REVOKED" && a.EntityId == created.Id);
    }

    [Fact]
    public async Task DeactivateUser_WhenSoleActiveAdministrator_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardId = await GetUserIdAsync(client, "BERNARD");

        // Bernard est le seul Administrateur actif du jeu de données (Lot1Seed) : le retrait de
        // son propre accès doit être bloqué (CLAUDE.md §17).
        var response = await client.PostAsync($"/api/v1/users/{bernardId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GrantAndRevokePermission_UpdatesUserPermissions()
    {
        var client = CreateClient(BernardIdentifiant);
        var created = await CreateTestUserAsync(client, roleSourceNom: "MISHRA");

        var granted = await client.PostAsync($"/api/v1/users/{created.Id}/permissions/AUDIT_VIEW", null);
        granted.StatusCode.Should().Be(HttpStatusCode.OK);

        var doubleGrant = await client.PostAsync($"/api/v1/users/{created.Id}/permissions/AUDIT_VIEW", null);
        doubleGrant.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var revoked = await client.DeleteAsync($"/api/v1/users/{created.Id}/permissions/AUDIT_VIEW");
        revoked.StatusCode.Should().Be(HttpStatusCode.OK);

        var doubleRevoke = await client.DeleteAsync($"/api/v1/users/{created.Id}/permissions/AUDIT_VIEW");
        doubleRevoke.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteTimeEntry_LogicallyDeletes_AndIsAudited()
    {
        var client = CreateClient(BernardIdentifiant);
        var bernardResourceId = await GetResourceIdAsync(client, "BERNARD");
        var runTypeId = await GetActivityTypeIdAsync(client, "RUN");

        var created = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = bernardResourceId, ActivityTypeId = runTypeId, Date = Today().AddDays(-1), DureeHeures = 7.75m
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = await created.Content.ReadFromJsonAsync<TimeEntryDto>();

        var deleted = await client.DeleteAsync($"/api/v1/time-entries/{entry!.Id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var doubleDelete = await client.DeleteAsync($"/api/v1/time-entries/{entry.Id}");
        doubleDelete.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var auditLogs = await client.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"/api/v1/audit-logs?entityType=TimeEntry&action=LOGICAL_DELETE&pageSize=200");
        auditLogs!.Items.Should().Contain(a => a.EntityId == entry.Id);
    }

    [Fact]
    public async Task RecalculateTimeEntry_WithoutPermission_Returns403()
    {
        var client = CreateClient(LegrandIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "BERNARD", "RUN");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/time-entries/{entryId}/recalculate", new TimeEntryRecalculationRequest { Reason = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RecalculateTimeEntry_WithoutReason_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "BERNARD", "RUN");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/time-entries/{entryId}/recalculate", new TimeEntryRecalculationRequest { Reason = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecalculateTimeEntry_WithPermissionAndReason_Returns200AndAuditsOldValue()
    {
        var client = CreateClient(BernardIdentifiant);
        var entryId = await GetTimeEntryIdAsync(client, "BERNARD", "RUN");

        var response = await client.PostAsJsonAsync(
            $"/api/v1/time-entries/{entryId}/recalculate",
            new TimeEntryRecalculationRequest { Reason = "Vérification après correction de TJM (test)." });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var snapshot = await response.Content.ReadFromJsonAsync<TimeEntryFinancialSnapshotDto>();
        snapshot!.CalculationStatus.Should().Be(SafranTimeTracker.Domain.Common.FinancialValuationStatus.Complete);

        var auditLogs = await client.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"/api/v1/audit-logs?entityType=TimeEntry&action=RECALCULATION&pageSize=200");
        var entry = auditLogs!.Items.Should().Contain(a => a.EntityId == entryId).Subject;
        entry.OldValue.Should().NotBeNullOrEmpty();
        entry.Reason.Should().Contain("Vérification");
    }

    [Fact]
    public async Task UpdateCompany_ChangesNomAndIsAudited()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyId = await GetCompanyIdAsync(client, "SAFRAN");
        var current = await client.GetFromJsonAsync<CompanyDto>($"/api/v1/companies/{companyId}");

        var response = await client.PutAsJsonAsync($"/api/v1/companies/{companyId}", new CompanyUpdateRequest
        {
            Nom = "SAFRAN (renommée pour test)",
            CompanyTypeId = current!.CompanyTypeId,
            ContactPrincipal = current.ContactPrincipal,
            EmailContact = current.EmailContact
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<CompanyDto>();
        dto!.Nom.Should().Be("SAFRAN (renommée pour test)");

        var auditLogs = await client.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"/api/v1/audit-logs?entityType=Company&action=UPDATE&pageSize=200");
        auditLogs!.Items.Should().Contain(a => a.EntityId == companyId);
    }

    [Fact]
    public async Task CreateOrderReceipt_ExceedingRemaining_Returns409WithoutOverridePermission()
    {
        var client = CreateClient(LegrandIdentifiant); // FINANCIAL_DATA_VIEW mais pas ORDER_RECEIPT_OVERRIDE
        var (orderId, _) = await CreateTestOrderAsync(client, budgetFinancierInitial: 10000m);

        var response = await client.PostAsJsonAsync($"/api/v1/orders/{orderId}/receipts", new OrderReceiptCreateRequest
        {
            ReceiptDate = Today(), ReceivedAmount = 15000m, Reason = "Dépassement volontaire (test)."
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateOrderReceipt_WithinRemaining_Returns201AndSummaryReflectsTotal()
    {
        var client = CreateClient(BernardIdentifiant);
        var (orderId, _) = await CreateTestOrderAsync(client, budgetFinancierInitial: 10000m);

        var response = await client.PostAsJsonAsync($"/api/v1/orders/{orderId}/receipts", new OrderReceiptCreateRequest
        {
            ReceiptDate = Today(), ReceivedAmount = 4000m, Reason = "Première réception (test)."
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var summary = await client.GetFromJsonAsync<OrderReceiptSummaryDto>($"/api/v1/orders/{orderId}/receipts/summary");
        summary!.TotalReceivedAmount.Should().Be(4000m);
        summary.RemainingReceivableAmount.Should().Be(6000m);
    }

    [Fact]
    public async Task CreateOrderReceipt_ExceedingRemaining_Returns201WithOverridePermission()
    {
        var client = CreateClient(BernardIdentifiant); // ORDER_RECEIPT_OVERRIDE (Lot6Seed)
        var (orderId, _) = await CreateTestOrderAsync(client, budgetFinancierInitial: 5000m);

        var response = await client.PostAsJsonAsync($"/api/v1/orders/{orderId}/receipts", new OrderReceiptCreateRequest
        {
            ReceiptDate = Today(), ReceivedAmount = 9000m, Reason = "Dépassement autorisé (test)."
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AuditLogs_WithoutAuditView_Returns403()
    {
        var client = CreateClient(LegrandIdentifiant);

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

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

    private static async Task<Guid> GetResourceIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        return result!.Items.First(r => r.Nom == nom).Id;
    }

    private static async Task<Guid> GetActivityTypeIdAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<SafranTimeTracker.Application.TimeTracking.Dtos.ActivityTypeDto>>(
            "/api/v1/activity-types?pageSize=100");
        return result!.Items.First(a => a.Code == code).Id;
    }

    private static async Task<Guid> GetTimeEntryIdAsync(HttpClient client, string resourceNom, string activityTypeCode)
    {
        var resourceId = await GetResourceIdAsync(client, resourceNom);
        var activityTypeId = await GetActivityTypeIdAsync(client, activityTypeCode);
        var result = await client.GetFromJsonAsync<PagedResult<TimeEntryDto>>($"/api/v1/time-entries?resourceId={resourceId}&pageSize=100");
        return result!.Items.First(t => t.ActivityTypeId == activityTypeId).Id;
    }

    private static async Task<Guid> GetCompanyIdAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=100");
        return result!.Items.First(c => c.Code == code).Id;
    }

    /// <summary>Crée un utilisateur de test dédié plutôt que de muter un utilisateur seedé
    /// réutilisé par d'autres tests de la même classe (CLAUDE.md §14, convention actée au Lot 5).</summary>
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

    private static async Task<(Guid OrderId, Guid CompanyId)> CreateTestOrderAsync(HttpClient client, decimal budgetFinancierInitial)
    {
        var companyId = await GetCompanyIdAsync(client, "SAFRAN");
        var response = await client.PostAsJsonAsync("/api/v1/orders", new OrderCreateRequest
        {
            Reference = $"CMD-TEST-{Guid.NewGuid():N}"[..20],
            Libelle = "Commande de test (réceptions)",
            CompanyId = companyId,
            BudgetFinancierInitial = budgetFinancierInitial,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinInitiale = new DateOnly(2026, 12, 31)
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = (await response.Content.ReadFromJsonAsync<OrderDto>())!;
        return (dto.Id, companyId);
    }
}
