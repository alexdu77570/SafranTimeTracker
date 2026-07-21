using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Budgets.Dtos;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Imports.Dtos;
using SafranTimeTracker.Application.Users.Dtos;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Imports;

namespace SafranTimeTracker.Tests.Integration.Api;

/// <summary>
/// Couvre le pipeline d'import du Lot 6 (cahier des charges §27) via l'API réelle : aperçu (sans
/// écriture), simulation (sans écriture), exécution (ImportBatch/ImportDiff persistés, §27.5-27.6),
/// import SharePoint simulé comparé au dernier lot confirmé (§27.4), et la protection par
/// permission (IMPORT_EXECUTE + FINANCIAL_DATA_VIEW pour un type financier, CLAUDE.md §12).
/// </summary>
public class Lot6ImportsTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140"; // IMPORT_EXECUTE + FINANCIAL_DATA_VIEW (Lot6Seed)
    private const string LegrandIdentifiant = "flegrand"; // ni IMPORT_EXECUTE ni USER_ADMINISTRATION

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
    public async Task GetTypes_ReturnsAllSixteenImportableTypes()
    {
        var client = CreateClient(BernardIdentifiant);

        var types = await client.GetFromJsonAsync<List<ImportTypeMetadataDto>>("/api/v1/imports/types");

        types.Should().HaveCount(Enum.GetValues<ImportEntityType>().Length);
        types!.Should().Contain(t => t.Type == ImportEntityType.Companies && t.SupportedModes.Contains(ImportMode.MiseAJour));
        types.Should().Contain(t => t.Type == ImportEntityType.Resources && t.SupportedModes.SequenceEqual(new[] { ImportMode.Ajout }));
    }

    [Fact]
    public async Task Preview_WithoutImportExecute_Returns403()
    {
        var client = CreateClient(LegrandIdentifiant);
        using var form = BuildForm(ImportEntityType.Applications, mode: null, "Nom,Code\nA,APP-X\n");

        var response = await client.PostAsync("/api/v1/imports/preview", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Preview_Applications_ReturnsDetectedHeadersWithoutPersisting()
    {
        var client = CreateClient(BernardIdentifiant);
        using var form = BuildForm(ImportEntityType.Applications, mode: null, "Nom,Code,ServiceId,Criticite\nApp Test,APP-TEST,,Basse\n");

        var response = await client.PostAsync("/api/v1/imports/preview", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var preview = await response.Content.ReadFromJsonAsync<ImportPreviewDto>();
        preview!.DetectedHeaders.Should().Contain("Nom");
        preview.LineCount.Should().Be(1);
    }

    [Fact]
    public async Task Simulate_CompanyAjout_ReportsAddWithoutPersisting()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyTypeId = await GetCompanyTypeIdAsync(client);
        var code = $"SIM-{Guid.NewGuid():N}"[..12];
        var csv = $"Nom,Code,CompanyTypeId,ContactPrincipal,EmailContact\nSociété Simulation,{code},{companyTypeId},Contact,contact@example.com\n";
        using var form = BuildForm(ImportEntityType.Companies, ImportMode.Ajout, csv);

        var response = await client.PostAsync("/api/v1/imports/simulate", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var simulation = await response.Content.ReadFromJsonAsync<ImportSimulationDto>();
        simulation!.AddCount.Should().Be(1);
        simulation.ErrorCount.Should().Be(0);

        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>($"/api/v1/companies?pageSize=200");
        companies!.Items.Should().NotContain(c => c.Code == code); // simulation : aucune écriture (§27.4)
    }

    [Fact]
    public async Task Execute_CompanyAjout_PersistsAndWritesImportBatchAndDiff()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyTypeId = await GetCompanyTypeIdAsync(client);
        var code = $"EXE-{Guid.NewGuid():N}"[..12];
        var csv = $"Nom,Code,CompanyTypeId,ContactPrincipal,EmailContact\nSociété Exécution,{code},{companyTypeId},Contact,contact@example.com\n";
        using var form = BuildForm(ImportEntityType.Companies, ImportMode.Ajout, csv);

        var response = await client.PostAsync("/api/v1/imports/execute", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var batch = await response.Content.ReadFromJsonAsync<ImportBatchDto>();
        batch!.AddCount.Should().Be(1);
        batch.Status.Should().Be(ImportBatchStatus.Confirme);

        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=200");
        companies!.Items.Should().Contain(c => c.Code == code);

        var diffs = await client.GetFromJsonAsync<PagedResult<ImportDiffDto>>($"/api/v1/imports/{batch.Id}/diffs");
        diffs!.Items.Should().ContainSingle(d => d.DiffType == ImportDiffType.Ajout);
    }

    [Fact]
    public async Task Execute_CompanyMiseAJour_WithIdColumn_ReturnsModificationDiff()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyTypeId = await GetCompanyTypeIdAsync(client);
        var code = $"UPD-{Guid.NewGuid():N}"[..12];

        var createCsv = $"Nom,Code,CompanyTypeId,ContactPrincipal,EmailContact\nAvant modification,{code},{companyTypeId},Contact,contact@example.com\n";
        using var createForm = BuildForm(ImportEntityType.Companies, ImportMode.Ajout, createCsv);
        var createResponse = await client.PostAsync("/api/v1/imports/execute", createForm);
        var companyId = (await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=200"))!
            .Items.First(c => c.Code == code).Id;

        var updateCsv = $"Id,Nom,CompanyTypeId,ContactPrincipal,EmailContact\n{companyId},Après modification,{companyTypeId},Contact,contact@example.com\n";
        using var updateForm = BuildForm(ImportEntityType.Companies, ImportMode.MiseAJour, updateCsv);
        var updateResponse = await client.PostAsync("/api/v1/imports/execute", updateForm);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var batch = await updateResponse.Content.ReadFromJsonAsync<ImportBatchDto>();
        batch!.UpdateCount.Should().Be(1);

        var updated = await client.GetFromJsonAsync<CompanyDto>($"/api/v1/companies/{companyId}");
        updated!.Nom.Should().Be("Après modification");

        var diffs = await client.GetFromJsonAsync<PagedResult<ImportDiffDto>>($"/api/v1/imports/{batch.Id}/diffs");
        diffs!.Items.Should().Contain(d => d.DiffType == ImportDiffType.Modification && d.FieldName == "Nom");
    }

    [Fact]
    public async Task Execute_UnsupportedModeForType_Returns409()
    {
        var client = CreateClient(BernardIdentifiant);
        using var form = BuildForm(ImportEntityType.Resources, ImportMode.Complet, "Nom,Prenom\nA,B\n");

        var response = await client.PostAsync("/api/v1/imports/execute", form);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Execute_FinancialType_WithoutFinancialDataView_Returns403EvenWithImportExecute()
    {
        var bernard = CreateClient(BernardIdentifiant);
        var testUser = await CreateTestUserAsync(bernard);
        var grantResponse = await bernard.PostAsync($"/api/v1/users/{testUser.Id}/permissions/IMPORT_EXECUTE", null);
        grantResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var client = CreateClient(testUser.Identifiant);
        using var form = BuildForm(ImportEntityType.Orders, ImportMode.Ajout, "Reference,Libelle,CompanyId,BudgetFinancierInitial,DateDebut,DateFinInitiale\n");

        var response = await client.PostAsync("/api/v1/imports/execute", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExecuteSharePoint_TwiceForSameType_SecondBatchReferencesFirstAsPrevious()
    {
        var client = CreateClient(BernardIdentifiant);
        var companyTypeId = await GetCompanyTypeIdAsync(client);
        var codeA = $"SPA-{Guid.NewGuid():N}"[..12];
        var codeB = $"SPB-{Guid.NewGuid():N}"[..12];

        using var firstForm = BuildForm(
            ImportEntityType.Companies, ImportMode.Ajout,
            $"Nom,Code,CompanyTypeId,ContactPrincipal,EmailContact\nSharePoint A,{codeA},{companyTypeId},Contact,contact@example.com\n");
        var firstResponse = await client.PostAsync("/api/v1/imports/sharepoint/execute", firstForm);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBatch = await firstResponse.Content.ReadFromJsonAsync<ImportBatchDto>();

        using var secondForm = BuildForm(
            ImportEntityType.Companies, ImportMode.Ajout,
            $"Nom,Code,CompanyTypeId,ContactPrincipal,EmailContact\nSharePoint B,{codeB},{companyTypeId},Contact,contact@example.com\n");
        var secondResponse = await client.PostAsync("/api/v1/imports/sharepoint/execute", secondForm);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondBatch = await secondResponse.Content.ReadFromJsonAsync<ImportBatchDto>();
        secondBatch!.PreviousBatchId.Should().Be(firstBatch!.Id);
        secondBatch.Source.Should().Be("SharePoint");
    }

    private static MultipartFormDataContent BuildForm(ImportEntityType type, ImportMode? mode, string csvContent)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(type.ToString()), "Type" }
        };
        if (mode is not null)
        {
            form.Add(new StringContent(mode.ToString()!), "Mode");
        }

        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        form.Add(fileContent, "File", "import.csv");

        return form;
    }

    private static async Task<Guid> GetCompanyTypeIdAsync(HttpClient client)
    {
        var companies = await client.GetFromJsonAsync<PagedResult<CompanyDto>>("/api/v1/companies?pageSize=200");
        return companies!.Items.First(c => c.Code == "SAFRAN").CompanyTypeId;
    }

    private static async Task<UserDto> CreateTestUserAsync(HttpClient client)
    {
        var users = await client.GetFromJsonAsync<PagedResult<UserDto>>("/api/v1/users?pageSize=100");
        var roleId = users!.Items.First(u => u.Nom == "MISHRA").RoleId;
        var identifiant = $"test-{Guid.NewGuid():N}"[..20];

        var response = await client.PostAsJsonAsync("/api/v1/users", new UserCreateRequest
        {
            Nom = "TEST", Prenom = "Import", Identifiant = identifiant, Email = $"{identifiant}@example.com",
            DateArrivee = DateOnly.FromDateTime(DateTime.UtcNow), RoleId = roleId
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<UserDto>())!;
    }
}

/// <summary>
/// Isolée dans sa propre fixture (base SQLite dédiée) : le mode Complet archive globalement toutes
/// les entités actives du type absentes du fichier (§27.2), ce qui interférerait avec les budgets
/// utilisés par d'autres classes de test si elle partageait leur base (CLAUDE.md §14, convention
/// actée au Lot 5).
/// </summary>
public class ImportCompleteModeTests(SafranTimeTrackerApiFactory factory) : IClassFixture<SafranTimeTrackerApiFactory>
{
    private const string BernardIdentifiant = "s636140";

    /// <summary>Lot 10 : le seed porte désormais plusieurs budgets actifs (enrichissement §35),
    /// pas un seul — le test vérifie que le mode Complet les clôture tous, pas seulement le premier.</summary>
    [Fact]
    public async Task Execute_BudgetsComplet_WithEmptyFile_ClosesAllSeededActiveBudgets()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(DemoCurrentUserProvider.DemoUserHeaderName, BernardIdentifiant);

        var budgets = await client.GetFromJsonAsync<PagedResult<BudgetDto>>("/api/v1/budgets?pageSize=200");
        var seededActiveBudgets = budgets!.Items.Where(b => b.Status == BudgetStatus.Actif).ToList();
        seededActiveBudgets.Should().NotBeEmpty();

        var form = new MultipartFormDataContent
        {
            { new StringContent(ImportEntityType.Budgets.ToString()), "Type" },
            { new StringContent(ImportMode.Complet.ToString()), "Mode" }
        };
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Id,Name,InitialAmount,StartDate\n"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        form.Add(fileContent, "File", "empty.csv");

        var response = await client.PostAsync("/api/v1/imports/execute", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var batch = await response.Content.ReadFromJsonAsync<ImportBatchDto>();
        batch!.DeleteCount.Should().Be(seededActiveBudgets.Count);

        foreach (var seededBudget in seededActiveBudgets)
        {
            var reloaded = await client.GetFromJsonAsync<BudgetDto>($"/api/v1/budgets/{seededBudget.Id}");
            reloaded!.Status.Should().Be(BudgetStatus.Cloture);
        }
    }
}
