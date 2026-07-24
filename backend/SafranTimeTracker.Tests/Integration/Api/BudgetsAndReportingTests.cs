using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Application.TimeTracking.Dtos;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 5 de bout en bout via l'API réelle (SQLite, migrations et seed appliqués) :
/// machine d'état des commandes (§13.2, Clôturée non réouvrable sans motif), rallonges (§13.3),
/// blocage de saisie sur commande clôturée sauf TIME_ENTRY_CORRECTION (§13.4), budgets et
/// versions (§14, ressource intégralement financière), tableau de bord (§25), reporting (§26,
/// exports réels), et clôture de l'écart Lot 4 sur les références liées à un projet (§17.7).
/// </summary>
public class BudgetsAndReportingTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // FINANCIAL_DATA_VIEW + TIME_ENTRY_CORRECTION (Lot5Seed)
    private const string ProjectElmCode = "PRJ-ELM-2026";

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
    public async Task OrderLifecycle_FollowsStateMachine_AndBlocksReactivationOfClosedOrderWithoutReopen()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyId = await GetSeededOrderCompanyIdAsync(client);

        var created = await client.PostAsJsonAsync("/api/v1/orders", new OrderCreateRequest
        {
            Reference = $"CMD-TEST-{Guid.NewGuid():N}"[..20],
            Libelle = "Commande de test (cycle de vie)",
            CompanyId = companyId,
            BudgetFinancierInitial = 10000m,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinInitiale = new DateOnly(2026, 12, 31)
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await created.Content.ReadFromJsonAsync<OrderDto>();
        var brouillonStatusId = order!.StatusId;

        // Suspendre depuis Brouillon est interdit (seul Active peut être suspendu).
        var suspendFromBrouillon = await client.PostAsync($"/api/v1/orders/{order.Id}/suspend", null);
        suspendFromBrouillon.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var activated = await client.PostAsync($"/api/v1/orders/{order.Id}/activate", null);
        activated.StatusCode.Should().Be(HttpStatusCode.OK);
        var activeStatusId = (await activated.Content.ReadFromJsonAsync<OrderDto>())!.StatusId;
        activeStatusId.Should().NotBe(brouillonStatusId);

        var suspended = await client.PostAsync($"/api/v1/orders/{order.Id}/suspend", null);
        var suspendedStatusId = (await suspended.Content.ReadFromJsonAsync<OrderDto>())!.StatusId;
        suspendedStatusId.Should().NotBe(activeStatusId);

        var reactivated = await client.PostAsync($"/api/v1/orders/{order.Id}/activate", null);
        (await reactivated.Content.ReadFromJsonAsync<OrderDto>())!.StatusId.Should().Be(activeStatusId);

        var closed = await client.PostAsync($"/api/v1/orders/{order.Id}/close", null);
        closed.StatusCode.Should().Be(HttpStatusCode.OK);

        // Clôturée ne peut pas redevenir Active via une transition ordinaire (§13.4).
        var activateAfterClose = await client.PostAsync($"/api/v1/orders/{order.Id}/activate", null);
        activateAfterClose.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var reopenWithoutMotif = await client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/reopen", new OrderReopenRequest { Motif = "" });
        reopenWithoutMotif.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var reopened = await client.PostAsJsonAsync(
            $"/api/v1/orders/{order.Id}/reopen", new OrderReopenRequest { Motif = "Réouverture pour correction (test)." });
        reopened.StatusCode.Should().Be(HttpStatusCode.OK);
        (await reopened.Content.ReadFromJsonAsync<OrderDto>())!.StatusId.Should().Be(activeStatusId);
    }

    /// <summary>Lot 11, décision 2 : même écart qu'CompanyType/Role (Lot 8), corrigé ici comme
    /// ProjectStatus (Lot 10) plutôt que contourné via knownReferentials.ts.</summary>
    [Fact]
    public async Task GetOrderStatuses_ReturnsSeededStatusesOrderedByOrdre()
    {
        var client = CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<OrderStatusDto>>("/api/v1/order-statuses?pageSize=100");

        result!.Items.Should().HaveCount(5);
        result.Items.Select(s => s.Code).Should().ContainInOrder("BROUILLON", "ACTIVE", "SUSPENDUE", "CONSOMMEE", "CLOTUREE");
    }

    [Fact]
    public async Task PostOrderExtension_IncreasesAdjustedBudgetAndEndDate_AndIsBlockedOnClosedOrder()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyId = await GetSeededOrderCompanyIdAsync(client);

        var created = await client.PostAsJsonAsync("/api/v1/orders", new OrderCreateRequest
        {
            Reference = $"CMD-EXT-{Guid.NewGuid():N}"[..20],
            Libelle = "Commande de test (rallonge)",
            CompanyId = companyId,
            BudgetFinancierInitial = 10000m,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinInitiale = new DateOnly(2026, 12, 31)
        });
        var order = await created.Content.ReadFromJsonAsync<OrderDto>();

        var extended = await client.PostAsJsonAsync($"/api/v1/orders/{order!.Id}/extensions", new OrderExtensionCreateRequest
        {
            AmountAdded = 2000m,
            NewEndDate = new DateOnly(2027, 3, 31),
            Reason = "Extension de périmètre (test)."
        });
        extended.StatusCode.Should().Be(HttpStatusCode.Created);

        var updatedOrder = await client.GetFromJsonAsync<OrderDto>($"/api/v1/orders/{order.Id}");
        updatedOrder!.BudgetFinancierAjuste.Should().Be(12000m);
        updatedOrder.DateFinAjustee.Should().Be(new DateOnly(2027, 3, 31));

        await client.PostAsync($"/api/v1/orders/{order.Id}/activate", null);
        await client.PostAsync($"/api/v1/orders/{order.Id}/close", null);

        var extensionOnClosed = await client.PostAsJsonAsync($"/api/v1/orders/{order.Id}/extensions", new OrderExtensionCreateRequest
        {
            AmountAdded = 500m,
            NewEndDate = new DateOnly(2027, 6, 30),
            Reason = "Ne devrait pas être appliquée (test)."
        });
        extensionOnClosed.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostTimeEntry_OnClosedOrder_IsBlockedWithoutCorrectionPermission_ButAllowedWith()
    {
        var adminClient = CreateClient(BernardIdentifiant);
        var companyId = await GetSeededOrderCompanyIdAsync(adminClient);

        var created = await adminClient.PostAsJsonAsync("/api/v1/orders", new OrderCreateRequest
        {
            Reference = $"CMD-CLOSED-{Guid.NewGuid():N}"[..20],
            Libelle = "Commande de test (clôturée)",
            CompanyId = companyId,
            BudgetFinancierInitial = 10000m,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinInitiale = new DateOnly(2026, 12, 31)
        });
        var order = await created.Content.ReadFromJsonAsync<OrderDto>();
        await adminClient.PostAsync($"/api/v1/orders/{order!.Id}/activate", null);
        await adminClient.PostAsync($"/api/v1/orders/{order.Id}/close", null);

        var resourceId = await GetResourceIdAsync(adminClient, "BERNARD"); // société interne, compatible
        var activityTypeId = await GetActivityTypeIdAsync(adminClient, "RUN");

        var withoutCorrectionClient = CreateClient(); // aucun en-tête -> aucune permission
        var blocked = await withoutCorrectionClient.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            OrderId = order.Id,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DureeHeures = 1m
        });
        blocked.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Bernard porte TIME_ENTRY_CORRECTION (Lot5Seed) : la même saisie doit passer.
        var allowed = await adminClient.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            OrderId = order.Id,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DureeHeures = 1m
        });
        allowed.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetBudget_WithFinancialPermission_ReturnsAggregatedFinancialFields()
    {
        var client = CreateClient(BernardIdentifiant);
        var budgetId = await GetSeededBudgetIdAsync(client);

        var dto = await client.GetFromJsonAsync<BudgetDto>($"/api/v1/budgets/{budgetId}");

        dto!.InitialAmount.Should().Be(150000.00m);
        dto.AdjustedAmount.Should().Be(180000.00m); // ajustement seedé (Lot5Seed)
        dto.CoutReelConsomme.Should().Be(700.00m); // saisie Legrand (Lot 3), même agrégat que ProjectDto
        dto.RisqueDepassement.Should().BeFalse(); // atterrissage MVP = coût réel << budget ajusté
    }

    [Fact]
    public async Task GetBudget_WithoutFinancialPermission_Returns403()
    {
        var client = CreateClient(); // pas d'en-tête -> pas de FINANCIAL_DATA_VIEW
        var budgetId = await GetSeededBudgetIdAsync(CreateClient(BernardIdentifiant));

        var response = await client.GetAsync($"/api/v1/budgets/{budgetId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostBudgetVersion_RecordsHistoryAndUpdatesAdjustedAmount()
    {
        // Budget dédié (plutôt que le budget seedé "Migration ELM") : évite toute interférence
        // avec GetBudget_WithFinancialPermission_ReturnsAggregatedFinancialFields, qui vérifie le
        // montant ajusté seedé (180000) — les tests de cette classe partagent la même base SQLite.
        var client = CreateClient(BernardIdentifiant);
        var budgetId = await CreateTestBudgetAsync(client, 50000.00m);

        var response = await client.PostAsJsonAsync($"/api/v1/budgets/{budgetId}/versions", new BudgetAdjustRequest
        {
            NewValue = 60000.00m,
            Reason = "Nouvel ajustement (test)."
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var version = await response.Content.ReadFromJsonAsync<BudgetVersionDto>();
        version!.OldValue.Should().Be(50000.00m);
        version.NewValue.Should().Be(60000.00m);

        var updatedBudget = await client.GetFromJsonAsync<BudgetDto>($"/api/v1/budgets/{budgetId}");
        updatedBudget!.AdjustedAmount.Should().Be(60000.00m);

        var versions = await client.GetFromJsonAsync<PagedResult<BudgetVersionDto>>($"/api/v1/budgets/{budgetId}/versions?pageSize=100");
        versions!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task CloseThenReactivateBudget_TransitionsStatusAndRejectsRedundantClose()
    {
        var client = CreateClient(BernardIdentifiant);
        var budgetId = await CreateTestBudgetAsync(client, 50000.00m);

        var closed = await client.PostAsync($"/api/v1/budgets/{budgetId}/close", null);
        closed.StatusCode.Should().Be(HttpStatusCode.OK);

        var closeAgain = await client.PostAsync($"/api/v1/budgets/{budgetId}/close", null);
        closeAgain.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var reactivated = await client.PostAsync($"/api/v1/budgets/{budgetId}/reactivate", null);
        reactivated.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Sous-lot 14.1 (rapport d'audit du Lot 14, constat BE-7) : avant
    /// ReportingFilterQueryValidator, ce cas faisait fuir une ArgumentException brute (500) depuis
    /// ReportingPeriodResolver au lieu du 400 attendu par la convention du projet (CLAUDE.md §12).</summary>
    [Fact]
    public async Task GetCharges_WithPersonnaliseePeriodAndNoDates_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);

        var response = await client.GetAsync("/api/v1/reporting/charges?periodType=Personnalisee");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCharges_WithPersonnaliseePeriodAndCustomToBeforeCustomFrom_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);

        var response = await client.GetAsync(
            "/api/v1/reporting/charges?periodType=Personnalisee&customFrom=2025-12-31&customTo=2024-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCharges_OverSeededPeriod_ReturnsExpectedTotalsAndDistinctReferenceCounts()
    {
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<ChargesReportDto>(
            "/api/v1/reporting/charges?periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        result!.ChargeTotaleHeures.Should().BeGreaterThan(0);
        result.NombreChanges.Should().Be(2); // CHG0001234 (2024) + CHG0009999 (2025), références distinctes
        result.TopProjets.Should().Contain(p => p.Nom == "Migration ELM");
    }

    /// <summary>Lot 12, décision 1 : évolution mensuelle et heatmap, mêmes saisies déjà chargées par
    /// GetChargesReportAsync, aucune nouvelle requête.</summary>
    [Fact]
    public async Task GetCharges_ReturnsMonthlyEvolutionAndHeatmap_ForSeededTimeEntry()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var result = await client.GetFromJsonAsync<ChargesReportDto>(
            $"/api/v1/reporting/charges?projectId={projectId}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        result!.EvolutionMensuelle.Should().ContainSingle(m => m.Annee == 2024 && m.Mois == 6 && m.ChargeTotaleHeures == 7.75m);
        result.Heatmap.Should().ContainSingle(h => h.WeekStartDate == new DateOnly(2024, 6, 10) && h.Nom.Contains("LEGRAND"));
    }

    /// <summary>Lot 12, décision 3 : agrégation dédiée "prévu vs réalisé" (docs/BACKLOG_METIER.md
    /// §16) — Ajustée (24 + 8 = 32h) prime sur Initiale (20 + 10 = 30h) pour Migration ELM, semaine
    /// du 2024-06-10 (Lot4Seed).</summary>
    [Fact]
    public async Task GetCharges_ReturnsPlanComparison_UsingAdjustedVersionWhenActive()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var result = await client.GetFromJsonAsync<ChargesReportDto>(
            $"/api/v1/reporting/charges?projectId={projectId}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        result!.PrevuVsRealise.ChargePrevue.Should().Be(32.00m);
        result.PrevuVsRealise.ChargeRealisee.Should().Be(result.ChargeTotaleHeures);
    }

    /// <summary>Lot 12, décision 3 : non calculable au niveau commande/type d'activité (la
    /// planification ne connaît pas ces dimensions) — jamais approché à zéro.</summary>
    [Fact]
    public async Task GetCharges_ReturnsNullPlanComparison_WhenFilteredByOrderOrActivityType()
    {
        var client = CreateClient(BernardIdentifiant);
        var activityTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");

        var result = await client.GetFromJsonAsync<ChargesReportDto>(
            $"/api/v1/reporting/charges?activityTypeId={activityTypeId}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        result!.PrevuVsRealise.ChargePrevue.Should().BeNull();
    }

    /// <summary>Lot 12, décision 2 : export réel du rapport opérationnel, même moteur générique que
    /// Charges/Financier (Lot 5) — aucune nouvelle logique de calcul.</summary>
    [Theory]
    [InlineData(ExportFormat.Csv, "text/csv")]
    [InlineData(ExportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(ExportFormat.Pdf, "application/pdf")]
    public async Task ExportOperational_ReturnsRealNonEmptyContentInRequestedFormat(ExportFormat format, string expectedContentType)
    {
        var client = CreateClient(BernardIdentifiant);

        var response = await client.GetAsync(
            $"/api/v1/reporting/operational/export?format={format}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be(expectedContentType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboard_ReturnsOperationalKpisAlways_AndFinancialOnlyWithPermission()
    {
        var withPermission = CreateClient(BernardIdentifiant);
        var withoutPermission = CreateClient();

        var withFinancial = await withPermission.GetFromJsonAsync<DashboardDto>("/api/v1/reporting/dashboard");
        // Migration ELM + Refonte VTOM (Lot4Seed) + Portail RUN ServiceNow + Support ServiceNow N2 +
        // Refonte Portail ELM (Lot10Seed, enrichissement §35) — Observabilité VTOM est Suspendu,
        // Consolidation Référentiels Terminé, Archive Legacy VTOM Archivé : les 3 ne comptent pas ici.
        withFinancial!.Operational.ProjetsActifs.Should().Be(5);
        withFinancial.Financial.Should().NotBeNull();

        var withoutFinancial = await withoutPermission.GetFromJsonAsync<DashboardDto>("/api/v1/reporting/dashboard");
        withoutFinancial!.Financial.Should().BeNull();
        withoutFinancial.Operational.ProjetsActifs.Should().Be(5);
    }

    /// <summary>Caractérisation avant refactoring (sous-lot 14.6 de l'audit du Lot 14) : verrouille
    /// la cohérence entre GetDashboardAsync (capacité agrégée, filtrée à une seule ressource) et
    /// GetOperationalReportAsync (capacité détaillée par ressource) avant de factoriser la boucle
    /// "disponibilité par ressource" (aujourd'hui dupliquée dans GetDashboardAsync,
    /// GetOperationalReportAsync et BuildWorkloadAlertsAsync) en un seul résolveur commun — les deux
    /// endpoints doivent continuer à produire des totaux identiques pour la même ressource et la
    /// même période après la factorisation.</summary>
    [Fact]
    public async Task GetDashboardAndOperationalReport_AgreeOnCapacity_ForTheSameScopedResourceAndPeriod()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "BERNARD");
        var query = $"resourceId={resourceId}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31";

        var dashboard = await client.GetFromJsonAsync<DashboardDto>($"/api/v1/reporting/dashboard?{query}");
        var operational = await client.GetFromJsonAsync<OperationalReportDto>($"/api/v1/reporting/operational?{query}");

        dashboard!.Operational.CapaciteTheorique.Should().BeGreaterThan(0);
        var capaciteRow = operational!.CapaciteEtDisponibilite.Should().ContainSingle(c => c.ResourceId == resourceId).Subject;
        dashboard.Operational.CapaciteTheorique.Should().Be(capaciteRow.CapaciteTheorique);
        dashboard.Operational.CapaciteReelle.Should().Be(capaciteRow.CapaciteReelle);
        dashboard.Operational.TauxDisponibilite.Should().Be(capaciteRow.TauxDisponibilite);
    }

    [Fact]
    public async Task GetFinancialReport_WithoutPermission_Returns403()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/reporting/financial");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Lot 11, décision 3 : ventilation mensuelle (§14.3 "consommation mensuelle"), calculée
    /// côté backend à partir des mêmes TimeEntryFinancialSnapshot que DifferentielParProjet/ParCommande/
    /// ParSociete/ParRessource (aucune duplication de logique financière).</summary>
    [Fact]
    public async Task GetFinancialReport_ReturnsMonthlyConsumption_ForPeriodContainingSnapshot()
    {
        var client = CreateClient(BernardIdentifiant);
        var resourceId = await GetResourceIdAsync(client, "BERNARD"); // société interne, TJM 650€ (Lot2Seed)
        var activityTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var created = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            Date = today, // le délai de modification (§19.4) bloque toute date trop ancienne
            DureeHeures = 7.75m, // = 1 jour à 7.75h/jour (HeuresParJour, Lot1Seed) -> coutReel = 650,00 €
            Reference = "INC0009998"
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var report = await client.GetFromJsonAsync<FinancialReportDto>("/api/v1/reporting/financial");

        var currentMonth = report!.ConsommationMensuelle.Should().ContainSingle(m => m.Annee == today.Year && m.Mois == today.Month).Subject;
        currentMonth.CoutReel.Should().BeGreaterOrEqualTo(650.00m);
        report.ConsommationMensuelle.Select(m => (m.Annee, m.Mois)).Should().OnlyHaveUniqueItems();
    }

    /// <summary>Caractérisation avant refactoring (sous-lot 14.6 de l'audit du Lot 14) : verrouille
    /// le comportement actuel des 4 ventilations différentielles (projet/commande/société/ressource,
    /// aujourd'hui 4 blocs dupliqués dans GetFinancialReportAsync) avant leur factorisation en un
    /// helper commun — aucun test existant n'asserte sur leur contenu, seulement sur
    /// ConsommationMensuelle (calculée à partir des mêmes snapshots).</summary>
    [Fact]
    public async Task GetFinancialReport_ReturnsMatchingDifferentialAcrossAllFourDimensions_ForSeededTimeEntry()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var orderId = await GetSeededOrderIdAsync(client);
        var resourceId = await GetResourceIdAsync(client, "BERNARD"); // société interne, TJM 650€ (Lot2Seed)
        var activityTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");

        var created = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            ProjectId = projectId,
            OrderId = orderId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DureeHeures = 7.75m, // = 1 jour à 7.75h/jour (HeuresParJour, Lot1Seed) -> coutReel = 650,00 €
            Reference = "INC0009997"
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var report = await client.GetFromJsonAsync<FinancialReportDto>("/api/v1/reporting/financial");

        report!.DifferentielParProjet.Should().Contain(d => d.Nom == "Migration ELM" && d.CoutReel >= 650.00m);
        report.DifferentielParCommande.Should().Contain(d => d.Nom == "CMD-2026-0001" && d.CoutReel >= 650.00m);
        report.DifferentielParSociete.Should().Contain(d => d.Nom == "SAFRAN" && d.CoutReel >= 650.00m);
        report.DifferentielParRessource.Should().Contain(d => d.Nom.Contains("BERNARD") && d.CoutReel >= 650.00m);

        // Chaque ventilation reste triée par |différentiel| décroissant (comportement actuel).
        report.DifferentielParProjet.Select(d => Math.Abs(d.Differentiel)).Should().BeInDescendingOrder();
        report.DifferentielParCommande.Select(d => Math.Abs(d.Differentiel)).Should().BeInDescendingOrder();
        report.DifferentielParSociete.Select(d => Math.Abs(d.Differentiel)).Should().BeInDescendingOrder();
        report.DifferentielParRessource.Select(d => Math.Abs(d.Differentiel)).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetProjectLinkedReferences_ReturnsReferencesRecordedOnProject()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var resourceId = await GetResourceIdAsync(client, "BERNARD");
        var activityTypeId = await GetActivityTypeIdAsync(client, "INCIDENT");

        var created = await client.PostAsJsonAsync("/api/v1/time-entries", new TimeEntryCreateRequest
        {
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            ProjectId = projectId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow), // le délai de modification (§19.4) bloque toute date trop ancienne
            DureeHeures = 3m,
            Reference = "INC0009999"
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var references = await client.GetFromJsonAsync<List<ProjectLinkedReferenceDto>>(
            $"/api/v1/reporting/projects/{projectId}/linked-references");

        references.Should().ContainSingle(r => r.Reference == "INC0009999" && r.ActivityTypeCode == "INCIDENT");
    }

    [Theory]
    [InlineData(ExportFormat.Csv, "text/csv")]
    [InlineData(ExportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData(ExportFormat.Pdf, "application/pdf")]
    public async Task ExportCharges_ReturnsRealNonEmptyContentInRequestedFormat(ExportFormat format, string expectedContentType)
    {
        var client = CreateClient(BernardIdentifiant);

        var response = await client.GetAsync(
            $"/api/v1/reporting/charges/export?format={format}&periodType=Personnalisee&customFrom=2024-01-01&customTo=2025-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be(expectedContentType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExportFinancial_WithoutPermission_Returns403()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/v1/reporting/financial/export?format=Csv");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDashboardKpis_ReturnsSeededReferentialRows()
    {
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<DashboardKpiDto>>("/api/v1/dashboard-kpis?pageSize=100");

        result!.Items.Should().Contain(k => k.Code == "BUDGET_RESTANT" && k.Category == DashboardKpiCategory.Financier);
        result.Items.Should().Contain(k => k.Code == "TEMPS_SAISIS" && k.Category == DashboardKpiCategory.Operationnel);
    }

    private static async Task<Guid> GetSeededOrderCompanyIdAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<PagedResult<OrderDto>>("/api/v1/orders?pageSize=100");
        return result!.Items.First(o => o.Reference == "CMD-2026-0001").CompanyId;
    }

    private static async Task<Guid> GetSeededOrderIdAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<PagedResult<OrderDto>>("/api/v1/orders?pageSize=100");
        return result!.Items.First(o => o.Reference == "CMD-2026-0001").Id;
    }

    private static async Task<Guid> GetSeededBudgetIdAsync(HttpClient client)
    {
        var result = await client.GetFromJsonAsync<PagedResult<BudgetDto>>("/api/v1/budgets?pageSize=100");
        return result!.Items.First(b => b.InitialAmount == 150000.00m).Id;
    }

    private static async Task<Guid> CreateTestBudgetAsync(HttpClient client, decimal initialAmount)
    {
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var response = await client.PostAsJsonAsync("/api/v1/budgets", new BudgetCreateRequest
        {
            Name = $"Budget de test {Guid.NewGuid():N}",
            ProjectId = projectId,
            InitialAmount = initialAmount,
            StartDate = new DateOnly(2026, 1, 1)
        });
        var dto = await response.Content.ReadFromJsonAsync<BudgetDto>();
        return dto!.Id;
    }

    private static async Task<Guid> GetProjectIdAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?pageSize=100");
        return result!.Items.First(p => p.Code == code).Id;
    }

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
}
