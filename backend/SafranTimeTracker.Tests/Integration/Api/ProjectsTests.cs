using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Milestones.Dtos;
using SafranTimeTracker.Application.Projects.Dtos;
using SafranTimeTracker.Application.Resources.Dtos;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le Lot 4 de bout en bout via l'API réelle (SQLite, migrations et seed appliqués) :
/// synthèse financière d'un projet (§16.2, agrégée depuis les saisies réelles, omise sans
/// FINANCIAL_DATA_VIEW), participants (§17.2, chevauchement -> 409), cycle de vie des versions de
/// planning (§18.3 — une seule Initiale, archivage automatique de l'ancienne version Ajustée
/// Active, précision actée avec l'utilisateur), synthèse écarts/risques (§29.5), jalons et
/// dérivation "en retard" (§24.2), archivage/réactivation de projet (§16.3).
/// </summary>
public class ProjectsTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // FINANCIAL_DATA_VIEW (Lot1Seed)
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
    public async Task GetProject_WithFinancialPermission_ReturnsAggregatedFinancialSummary()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var dto = await client.GetFromJsonAsync<ProjectDto>($"/api/v1/projects/{projectId}");

        dto!.FinancialSummary.Should().NotBeNull();
        dto.FinancialSummary!.BudgetInitial.Should().Be(150000.00m);
        dto.FinancialSummary.CoutReelConsomme.Should().Be(700.00m); // saisie Legrand seedée (Lot 3)
        dto.FinancialSummary.CoutContractuelConsomme.Should().Be(750.00m);
        dto.FinancialSummary.Differentiel.Should().Be(50.00m);
        dto.FinancialSummary.BudgetRestant.Should().Be(150000.00m - 700.00m);
    }

    [Fact]
    public async Task GetProject_WithoutFinancialPermission_OmitsFinancialSummary()
    {
        var client = CreateClient();
        var projectId = await GetProjectIdAsync(CreateClient(BernardIdentifiant), ProjectElmCode);

        var dto = await client.GetFromJsonAsync<ProjectDto>($"/api/v1/projects/{projectId}");

        dto!.FinancialSummary.Should().BeNull();
    }

    [Fact]
    public async Task GetParticipants_WithFinancialPermission_ReturnsApplicableTjm()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var result = await client.GetFromJsonAsync<PagedResult<ProjectParticipantDto>>(
            $"/api/v1/projects/{projectId}/participants?pageSize=100");

        var legrandResourceId = await GetResourceIdAsync(client, "LEGRAND");
        var legrandParticipant = result!.Items.Should().ContainSingle(p => p.ResourceId == legrandResourceId).Subject;
        legrandParticipant.FinancialSummary!.TjmPersonneApplicable.Should().Be(700.00m);
    }

    [Fact]
    public async Task PostParticipant_WithOverlappingPeriod_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var georgesId = await GetResourceIdAsync(client, "GEORGES"); // déjà participant depuis 2024-01-01 (seed)

        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/participants", new ProjectParticipantCreateRequest
        {
            ResourceId = georgesId, DateDebut = new DateOnly(2024, 6, 1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetPlanningSynthesis_ForSeededProject_ReturnsExpectedChargeMetrics()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var synthesis = await client.GetFromJsonAsync<ProjectPlanningSynthesisDto>($"/api/v1/projects/{projectId}/planning");

        synthesis!.ChargeInitiale.Should().Be(30.00m); // 20 (Georges) + 10 (Legrand), version Initiale
        synthesis.ChargeAjustee.Should().Be(32.00m); // 24 + 8, version Ajustée active
        synthesis.ChargeConsommee.Should().Be(7.75m); // saisie réelle Legrand
        synthesis.ChargeRestante.Should().Be(32.00m - 7.75m);
        synthesis.DeriveCharge.Should().Be(2.00m); // 32 - 30
        synthesis.RisquePlanning.Should().BeTrue(); // date de fin ajustée (2025-03-31) > initiale (2024-12-31)
        synthesis.RisqueBudget.Should().BeFalse(); // atterrissage financier extrapolé (~2890) << budget ajusté (180000, Lot 5)
    }

    [Fact]
    public async Task GetPlanningSynthesis_WithoutFinancialPermission_OmitsRisqueBudgetOnly()
    {
        var client = CreateClient();
        var projectId = await GetProjectIdAsync(CreateClient(BernardIdentifiant), ProjectElmCode);

        var synthesis = await client.GetFromJsonAsync<ProjectPlanningSynthesisDto>($"/api/v1/projects/{projectId}/planning");

        synthesis!.RisqueBudget.Should().BeNull();
        synthesis.ChargeInitiale.Should().Be(30.00m); // le reste de la synthèse n'est pas financier
    }

    [Fact]
    public async Task PostInitialPlanVersion_WhenOneAlreadyExists_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode); // possède déjà une version Initiale (seed)

        var response = await client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/plan-versions/initial", new ProjectPlanVersionCreateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostAdjustedPlanVersion_ArchivesPreviousActiveAdjustedVersion()
    {
        // Précision actée avec l'utilisateur (Lot 4) : une seule version Ajustée Active à la fois ;
        // l'ancienne (seedée, PlanVersionElmAjuste) doit basculer à Archivee automatiquement.
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var versionsBefore = await client.GetFromJsonAsync<PagedResult<ProjectPlanVersionDto>>(
            $"/api/v1/projects/{projectId}/plan-versions?pageSize=100");
        var previousActiveAdjusted = versionsBefore!.Items.Should()
            .ContainSingle(v => v.Type == ProjectPlanVersionType.Ajuste && v.Statut == ProjectPlanVersionStatus.Active).Subject;

        var response = await client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/plan-versions/adjusted",
            new ProjectPlanVersionAdjustmentRequest { Motif = "Nouvelle ré-estimation (test)." });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var newVersion = await response.Content.ReadFromJsonAsync<ProjectPlanVersionDto>();
        newVersion!.Statut.Should().Be(ProjectPlanVersionStatus.Active);

        var versionsAfter = await client.GetFromJsonAsync<PagedResult<ProjectPlanVersionDto>>(
            $"/api/v1/projects/{projectId}/plan-versions?pageSize=100");
        versionsAfter!.Items.Should().Contain(v => v.Id == previousActiveAdjusted.Id && v.Statut == ProjectPlanVersionStatus.Archivee);
        versionsAfter.Items.Where(v => v.Type == ProjectPlanVersionType.Ajuste && v.Statut == ProjectPlanVersionStatus.Active)
            .Should().ContainSingle().Which.Id.Should().Be(newVersion.Id);
    }

    [Fact]
    public async Task PostAdjustedPlanVersion_WithoutMotif_Returns400()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var response = await client.PostAsJsonAsync(
            $"/api/v1/projects/{projectId}/plan-versions/adjusted", new ProjectPlanVersionAdjustmentRequest { Motif = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMilestones_FilteredByEnRetard_ReturnsOnlyLateOnes()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var result = await client.GetFromJsonAsync<PagedResult<MilestoneDto>>(
            $"/api/v1/milestones?projectId={projectId}&enRetard=true&pageSize=100");

        result!.Items.Should().OnlyContain(m => m.EstEnRetard);
        result.Items.Should().Contain(m => m.Nom == "GO PROD Migration ELM");
        result.Items.Should().NotContain(m => m.Nom == "Kick-off Migration ELM"); // Terminé -> jamais en retard
        result.Items.Should().NotContain(m => m.Nom == "CAB Migration ELM"); // date future
    }

    [Fact]
    public async Task ArchiveThenReactivateProject_TransitionsStatusAndRejectsRedundantArchive()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, "PRJ-VTOM-2026");

        var archived = await client.PostAsync($"/api/v1/projects/{projectId}/archive", null);
        archived.StatusCode.Should().Be(HttpStatusCode.OK);

        var archiveAgain = await client.PostAsync($"/api/v1/projects/{projectId}/archive", null);
        archiveAgain.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var reactivated = await client.PostAsync($"/api/v1/projects/{projectId}/reactivate", null);
        reactivated.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostProject_WithValidData_CreatesActiveProject()
    {
        var client = CreateClient(BernardIdentifiant);
        var existing = await client.GetFromJsonAsync<ProjectDto>(
            $"/api/v1/projects/{await GetProjectIdAsync(client, ProjectElmCode)}");
        var piloteId = await GetResourceIdAsync(client, "REAU");

        var response = await client.PostAsJsonAsync("/api/v1/projects", new ProjectCreateRequest
        {
            Nom = "Projet de test",
            Code = $"PRJ-TEST-{Guid.NewGuid():N}"[..16],
            ApplicationId = existing!.ApplicationId,
            PiloteId = piloteId,
            DepartmentId = existing.DepartmentId,
            ServiceId = existing.ServiceId,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinPrevueInitiale = new DateOnly(2026, 12, 31)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProjectDto>();
        created!.FinancialSummary.Should().NotBeNull(); // Bernard a FINANCIAL_DATA_VIEW
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
}
