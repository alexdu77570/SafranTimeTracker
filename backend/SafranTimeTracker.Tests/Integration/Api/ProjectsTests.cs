using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Applications.Dtos;
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

    /// <summary>Lot 11, §24.3 "filtre application" : paramètre optionnel ajouté à une action déjà
    /// existante (même précédent que les filtres étendus des Lots 9/10), aucune nouvelle entité.</summary>
    [Fact]
    public async Task GetMilestones_FilteredByApplicationId_ReturnsOnlyMatchingApplication()
    {
        var client = CreateClient(BernardIdentifiant);
        var applications = await client.GetFromJsonAsync<PagedResult<ApplicationReferenceDto>>(
            "/api/v1/applications?pageSize=100");
        var appId = applications!.Items.Single(a => a.Nom == "VTOM").Id;

        var result = await client.GetFromJsonAsync<PagedResult<MilestoneDto>>(
            $"/api/v1/milestones?applicationId={appId}&pageSize=100");

        result!.Items.Should().HaveCount(5);
        result.Items.Should().OnlyContain(m => m.ApplicationId == appId);
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

    private static async Task<ProjectDto> GetProjectByCodeAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?pageSize=100");
        return result!.Items.First(p => p.Code == code);
    }

    private static async Task<Guid> GetResourceIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        return result!.Items.First(r => r.Nom == nom).Id;
    }
}

/// <summary>
/// Couvre les évolutions backend du Lot 10 (§16.1 : filtres de liste étendus ; §18.2 : vue transverse
/// "Planning projet" ; lecture des lignes hebdomadaires) sur le jeu de démonstration enrichi
/// (Lot10Seed, §35). Classe séparée de <see cref="ProjectsTests"/> pour ne pas mélanger les
/// assertions du Lot 4 (jeu minimal) avec celles portant sur le jeu enrichi de ce lot.
/// </summary>
public class ProjectPlanningLot10Tests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
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
    public async Task GetProjectStatuses_ReturnsSeededStatusesOrderedByOrdre()
    {
        var client = CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProjectStatusDto>>("/api/v1/project-statuses?pageSize=100");

        result!.Items.Should().HaveCount(4);
        result.Items.Select(s => s.Code).Should().ContainInOrder("ACTIF", "SUSPENDU", "TERMINE", "ARCHIVE");
    }

    [Fact]
    public async Task GetProjects_ReturnsAtLeastEightSeededProjects()
    {
        // §35 : jeu de démonstration minimal de 8 projets (Lot4Seed : 2, Lot10Seed : 6 supplémentaires).
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?pageSize=100");

        result!.TotalCount.Should().BeGreaterThanOrEqualTo(8);
    }

    [Fact]
    public async Task GetProjects_FilteredByTeamId_ReturnsOnlyMatchingProjects()
    {
        var client = CreateClient(BernardIdentifiant);
        var elm = await GetProjectByCodeAsync(client, ProjectElmCode); // TeamProjetsA (Lot4Seed)

        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>($"/api/v1/projects?teamId={elm.TeamId}&pageSize=100");

        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(p => p.TeamId == elm.TeamId);
    }

    [Fact]
    public async Task GetProjects_FilteredByNiveauRisque_ReturnsOnlyMatchingProjects()
    {
        var client = CreateClient(BernardIdentifiant);

        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?niveauRisque=Eleve&pageSize=100");

        result!.Items.Should().Contain(p => p.Code == "PRJ-SNOW-2025"); // Portail RUN ServiceNow (Lot10Seed)
        result.Items.Should().OnlyContain(p => p.NiveauRisque == ProjectRiskLevel.Eleve);
    }

    [Fact]
    public async Task GetProjects_FilteredByPeriode_ExcludesProjectsOutsideRange()
    {
        var client = CreateClient(BernardIdentifiant);

        // Archive Legacy VTOM (2023, Lot10Seed) est hors de cette fenêtre 2024-2026.
        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>(
            "/api/v1/projects?from=2024-01-01&to=2026-12-31&pageSize=100");

        result!.Items.Should().NotContain(p => p.Code == "PRJ-VTOM-LEGACY-2023");
    }

    [Fact]
    public async Task GetProjects_FilteredByAlertePlanning_ReturnsOnlyProjectsWithAdjustedDateAfterInitial()
    {
        var client = CreateClient(BernardIdentifiant);

        var withAlert = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?alertePlanning=true&pageSize=100");

        withAlert!.Items.Should().Contain(p => p.Code == ProjectElmCode); // DateFinAjustee > DateFinPrevueInitiale (Lot4Seed)
        withAlert.Items.Should().Contain(p => p.Code == "PRJ-VTOM-OBS-2024"); // Observabilité VTOM (Lot10Seed)
        withAlert.Items.Should().OnlyContain(p => p.DateFinAjustee != null && p.DateFinAjustee > p.DateFinPrevueInitiale);

        var withoutAlert = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?alertePlanning=false&pageSize=100");
        withoutAlert!.Items.Should().NotContain(p => p.Code == ProjectElmCode);
    }

    [Fact]
    public async Task GetProjects_FilteredByAlerteBudget_WithoutFinancialPermission_ReturnsEmpty()
    {
        // RisqueBudget dépend de données financières : sans FINANCIAL_DATA_VIEW, le filtre ne peut
        // honnêtement retenir aucun projet plutôt que d'ignorer silencieusement le filtre demandé.
        var client = CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?alerteBudget=true&pageSize=100");

        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProjects_FilteredByAlerteBudget_WithFinancialPermission_ReflectsPlanningSynthesisRisqueBudget()
    {
        var client = CreateClient(BernardIdentifiant);

        // Migration ELM a RisqueBudget = false (atterrissage ~2890 << budget ajusté 180000, cf.
        // ProjectsTests.GetPlanningSynthesis_ForSeededProject_ReturnsExpectedChargeMetrics) : le
        // filtre alerteBudget=false doit donc le retenir, alerteBudget=true jamais.
        var withoutAlert = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?alerteBudget=false&pageSize=100");
        withoutAlert!.Items.Should().Contain(p => p.Code == ProjectElmCode);

        var withAlert = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?alerteBudget=true&pageSize=100");
        withAlert!.Items.Should().NotContain(p => p.Code == ProjectElmCode);
    }

    [Fact]
    public async Task GetWeeklyPlans_ForSeededInitialVersion_ReturnsSeededLines()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var versions = await client.GetFromJsonAsync<PagedResult<ProjectPlanVersionDto>>(
            $"/api/v1/projects/{projectId}/plan-versions?pageSize=100");
        var initialVersion = versions!.Items.Single(v => v.Type == ProjectPlanVersionType.Initial);

        var lines = await client.GetFromJsonAsync<List<ProjectWeeklyPlanDto>>(
            $"/api/v1/projects/{projectId}/plan-versions/{initialVersion.Id}/weekly-plans");

        lines.Should().HaveCount(2); // Georges 20h + Legrand 10h, semaine du 2024-06-10 (Lot4Seed)
        lines!.Sum(l => l.ChargePlanifieeHeures).Should().Be(30.00m);
    }

    [Fact]
    public async Task GetProjectPlanningOverview_FilteredByProjectId_ReturnsWeeklyRowsForThatProjectOnly()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);

        var result = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>(
            $"/api/v1/project-planning?projectId={projectId}&pageSize=100");

        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(r => r.ProjectId == projectId);
        result.Items.Should().Contain(r => r.ChargePlanifieeAjustee == 24.00m); // Georges, semaine du 2024-06-10 (Lot4Seed)
    }

    [Fact]
    public async Task GetProjectPlanningOverview_ComputesChargeRealiseeFromTimeEntriesNotManualEntry()
    {
        var client = CreateClient(BernardIdentifiant);
        var projectId = await GetProjectIdAsync(client, ProjectElmCode);
        var legrandId = await GetResourceIdAsync(client, "LEGRAND");

        var result = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>(
            $"/api/v1/project-planning?projectId={projectId}&resourceId={legrandId}&pageSize=100");

        // TimeEntryLegrandProjet (Lot3Seed, 7.75h, 2024-06-10) : le "réalisé" provient exclusivement
        // de cette saisie de temps, jamais d'une troisième version saisie manuellement (§18.3).
        var row = result!.Items.Should().ContainSingle().Subject;
        row.ChargeRealisee.Should().Be(7.75m);
        row.ChargePlanifieeAjustee.Should().Be(8.00m); // Lot4Seed
        row.EcartPrevuRealise.Should().Be(7.75m - 8.00m);
    }

    [Fact]
    public async Task GetProjectPlanningOverview_FilteredByResourceId_ReturnsRowsAcrossMultipleProjects()
    {
        var client = CreateClient(BernardIdentifiant);
        var nguyenId = await GetResourceIdAsync(client, "NGUYEN"); // participant planifié sur 2 projets (Lot10Seed)

        var result = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>(
            $"/api/v1/project-planning?resourceId={nguyenId}&pageSize=100");

        result!.Items.Should().OnlyContain(r => r.ResourceId == nguyenId);
        result.Items.Select(r => r.ProjectId).Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task GetProjectPlanningOverview_FilteredBySurcharge_PartitionsRowsWithoutLoss()
    {
        var client = CreateClient(BernardIdentifiant);

        var all = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>("/api/v1/project-planning?pageSize=200");
        var surcharged = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>("/api/v1/project-planning?surcharge=true&pageSize=200");
        var notSurcharged = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>("/api/v1/project-planning?surcharge=false&pageSize=200");

        all!.TotalCount.Should().Be(surcharged!.TotalCount + notSurcharged!.TotalCount);
        // .All() plutôt que FluentAssertions .OnlyContain() : vrai vacuement si l'un des deux
        // sous-ensembles est vide (aucune surcharge dans les données de démonstration n'est pas
        // une anomalie — seule l'absence de perte lors du partitionnement est vérifiée ci-dessus).
        surcharged!.Items.All(r => r.Surcharge).Should().BeTrue();
        notSurcharged!.Items.All(r => !r.Surcharge).Should().BeTrue();
    }

    /// <summary>Sous-lot 14.4 de l'audit du Lot 14 : la pagination (sans filtre surcharge) est
    /// désormais bornée en base sur les clés (Projet, Ressource, Semaine) distinctes plutôt que
    /// matérialisée intégralement puis paginée en mémoire — vérifie l'absence de perte/doublon en
    /// parcourant toutes les pages d'une seule ligne.</summary>
    [Fact]
    public async Task GetProjectPlanningOverview_PagedOnePerPage_CoversAllRowsWithoutDuplication()
    {
        var client = CreateClient(BernardIdentifiant);

        var all = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>("/api/v1/project-planning?pageSize=200");
        all!.TotalCount.Should().BeGreaterThan(1); // jeu de démonstration multi-projets/ressources/semaines

        var seen = new List<(Guid ProjectId, Guid ResourceId, DateOnly WeekStartDate)>();
        for (var page = 1; page <= all.TotalCount; page++)
        {
            var result = await client.GetFromJsonAsync<PagedResult<ProjectPlanningRowDto>>(
                $"/api/v1/project-planning?page={page}&pageSize=1");
            result!.TotalCount.Should().Be(all.TotalCount);
            var row = result.Items.Should().ContainSingle().Subject;
            seen.Add((row.ProjectId, row.ResourceId, row.WeekStartDate));
        }

        seen.Should().OnlyHaveUniqueItems();
        seen.Should().HaveCount(all.TotalCount);
    }

    private static async Task<Guid> GetProjectIdAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?pageSize=100");
        return result!.Items.First(p => p.Code == code).Id;
    }

    private static async Task<ProjectDto> GetProjectByCodeAsync(HttpClient client, string code)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ProjectDto>>("/api/v1/projects?pageSize=100");
        return result!.Items.First(p => p.Code == code);
    }

    private static async Task<Guid> GetResourceIdAsync(HttpClient client, string nom)
    {
        var result = await client.GetFromJsonAsync<PagedResult<ResourceDto>>("/api/v1/resources?pageSize=100");
        return result!.Items.First(r => r.Nom == nom).Id;
    }
}
