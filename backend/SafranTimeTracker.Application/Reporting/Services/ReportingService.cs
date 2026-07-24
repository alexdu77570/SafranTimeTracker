using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Capacity.Services;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Time;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Reporting.Services;

/// <summary>
/// Cahier des charges §21 (Charges), §25 (Tableau de bord) et §26 (Reporting). Toutes les
/// agrégations vivent ici, jamais dans un contrôleur (demande explicite de l'utilisateur, Lot 5).
/// Réutilise AvailabilityService (Lot 3) pour la capacité théorique/réelle — jamais recalculée ici
/// — et ProjectPlanningCalculator (Lot 4/5) pour le risque budgétaire. Les décomptes par référence
/// opérationnelle (Incidents/Changes/...) comptent des <c>TimeEntry.Reference</c> distinctes, pas
/// des lignes de saisie (une référence peut porter plusieurs saisies, §19.3).
/// </summary>
public class ReportingService(
    IReadRepository<TimeEntry> timeEntryRepository,
    IReadRepository<TimeEntryFinancialSnapshot> snapshotRepository,
    IReadRepository<Resource> resourceRepository,
    IReadRepository<ActivityType> activityTypeRepository,
    IReadRepository<Project> projectRepository,
    IReadRepository<Order> orderRepository,
    IReadRepository<Budget> budgetRepository,
    IReadRepository<Milestone> milestoneRepository,
    IReadRepository<Company> companyRepository,
    IReadRepository<Department> departmentRepository,
    IReadRepository<Service> serviceRepository,
    IReadRepository<Team> teamRepository,
    IReadRepository<SettingsEntity> settingsRepository,
    IReadRepository<ProjectWeeklyPlan> weeklyPlanRepository,
    AvailabilityService availabilityService,
    ICurrentUser currentUser)
{
    private const string CodeIncident = "INCIDENT";
    private const string CodeChange = "CHANGE";
    private const string CodeProblem = "PROBLEM";
    private const string CodeRitm = "RITM";
    private const string CodeVabe = "VABE";
    private const string CodeVsr = "VSR";

    public async Task<ChargesReportDto> GetChargesReportAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var (from, to) = ResolvePeriod(filter);
        var entries = await BuildFilteredQuery(filter, from, to)
            .Select(t => new { t.ResourceId, t.ActivityTypeId, t.ProjectId, t.OrderId, t.Reference, t.DureeHeures, t.Date })
            .ToListAsync(cancellationToken);

        var activityTypes = await LoadActivityTypesAsync(entries.Select(e => e.ActivityTypeId), cancellationToken);

        var chargeTotale = entries.Sum(e => e.DureeHeures);
        var chargeRun = entries.Where(e => activityTypes.GetValueOrDefault(e.ActivityTypeId)?.IsRun == true).Sum(e => e.DureeHeures);

        var topApplications = await BuildTopApplicationsAsync(entries.Select(e => (e.ProjectId, e.DureeHeures)), cancellationToken);
        var topProjets = await BuildTopProjectsAsync(entries.Select(e => (e.ProjectId, e.DureeHeures)), cancellationToken);
        var topCommandes = await BuildTopOrdersAsync(entries.Select(e => (e.OrderId, e.DureeHeures)), cancellationToken);
        var topUtilisateurs = await BuildTopResourcesAsync(entries.Select(e => (e.ResourceId, e.DureeHeures)), cancellationToken);
        var chargesScopedResourceIds = await ResolveScopedResourceIdsAsync(filter, cancellationToken);
        var chargesAvailabilities = await ResolveAvailabilitiesAsync(chargesScopedResourceIds, from, to, cancellationToken);
        var (surcharges, sousCharges) = await BuildWorkloadAlertsAsync(
            chargesScopedResourceIds, chargesAvailabilities, entries.Select(e => (e.ResourceId, e.DureeHeures)), cancellationToken);

        var evolutionMensuelle = entries
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new ChargesMonthlyEvolutionDto
            {
                Annee = g.Key.Year,
                Mois = g.Key.Month,
                ChargeTotaleHeures = g.Sum(e => e.DureeHeures),
                ChargeRunHeures = g.Where(e => activityTypes.GetValueOrDefault(e.ActivityTypeId)?.IsRun == true).Sum(e => e.DureeHeures),
                ChargeHorsRunHeures = g.Where(e => activityTypes.GetValueOrDefault(e.ActivityTypeId)?.IsRun != true).Sum(e => e.DureeHeures)
            })
            .OrderBy(m => m.Annee).ThenBy(m => m.Mois)
            .ToList();

        var heatmapGroups = entries
            .GroupBy(e => (e.ResourceId, WeekStart: GetWeekStart(e.Date)))
            .Select(g => (g.Key.ResourceId, g.Key.WeekStart, ChargeHeures: g.Sum(e => e.DureeHeures)))
            .ToList();
        var heatmapResourceIds = heatmapGroups.Select(g => g.ResourceId).Distinct().ToList();
        var heatmapResourceNames = await resourceRepository.Query()
            .Where(r => heatmapResourceIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => $"{r.Prenom} {r.Nom}", cancellationToken);
        var heatmap = heatmapGroups
            .Select(g => new ChargesHeatmapEntryDto
            {
                ResourceId = g.ResourceId,
                Nom = heatmapResourceNames.GetValueOrDefault(g.ResourceId, g.ResourceId.ToString()),
                WeekStartDate = g.WeekStart,
                ChargeHeures = g.ChargeHeures
            })
            .OrderBy(h => h.WeekStartDate).ThenBy(h => h.Nom)
            .ToList();

        var prevuVsRealise = await BuildPlanComparisonAsync(filter, from, to, chargeTotale, cancellationToken);

        return new ChargesReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            ChargeTotaleHeures = chargeTotale,
            ChargeRunHeures = chargeRun,
            ChargeHorsRunHeures = chargeTotale - chargeRun,
            NombreIncidents = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeIncident),
            NombreChanges = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeChange),
            NombreProblems = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeProblem),
            NombreRitm = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeRitm),
            NombreVabe = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeVabe),
            NombreVsr = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeVsr),
            TopApplications = topApplications,
            TopUtilisateurs = topUtilisateurs,
            TopProjets = topProjets,
            TopCommandes = topCommandes,
            RessourcesSurchargees = surcharges,
            RessourcesSousChargees = sousCharges,
            EvolutionMensuelle = evolutionMensuelle,
            Heatmap = heatmap,
            PrevuVsRealise = prevuVsRealise
        };
    }

    public async Task<DashboardDto> GetDashboardAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var (from, to) = ResolvePeriod(filter);
        var scopedResourceIds = await ResolveScopedResourceIdsAsync(filter, cancellationToken);
        var availabilities = await ResolveAvailabilitiesAsync(scopedResourceIds, from, to, cancellationToken);

        var capaciteTheorique = availabilities.Values.Sum(a => a.CapaciteTheorique);
        var capaciteReelle = availabilities.Values.Sum(a => a.CapaciteReelle);

        var entries = await BuildFilteredQuery(filter, from, to)
            .Select(t => new { t.ResourceId, t.ActivityTypeId, t.Reference, t.DureeHeures })
            .ToListAsync(cancellationToken);
        var activityTypes = await LoadActivityTypesAsync(entries.Select(e => e.ActivityTypeId), cancellationToken);

        var chargeRun = entries.Where(e => activityTypes.GetValueOrDefault(e.ActivityTypeId)?.IsRun == true).Sum(e => e.DureeHeures);
        var chargeTotale = entries.Sum(e => e.DureeHeures);
        var (surcharges, sousCharges) = await BuildWorkloadAlertsAsync(
            scopedResourceIds, availabilities, entries.Select(e => (e.ResourceId, e.DureeHeures)), cancellationToken);

        var operational = new DashboardOperationalKpisDto
        {
            TempsSaisisHeures = chargeTotale,
            CapaciteTheorique = capaciteTheorique,
            CapaciteReelle = capaciteReelle,
            TauxDisponibilite = capaciteTheorique > 0 ? Math.Round(capaciteReelle / capaciteTheorique * 100, 2) : 0m,
            ChargeRunHeures = chargeRun,
            ChargeHorsRunHeures = chargeTotale - chargeRun,
            IncidentsOuverts = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeIncident),
            ChangesEnCours = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeChange),
            ProblemsOuverts = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeProblem),
            RitmEnCours = CountDistinctReferences(entries.Select(e => (e.ActivityTypeId, e.Reference)), activityTypes, CodeRitm),
            ProjetsActifs = await projectRepository.Query().Where(p => p.Status.Code == "ACTIF").CountAsync(cancellationToken),
            JalonsEnRetard = (await GetMilestonesEnRetardAsync(cancellationToken)).Count,
            RessourcesSurchargees = surcharges.Count,
            RessourcesSousChargees = sousCharges.Count
        };

        var financial = currentUser.HasPermission(PermissionCodes.FinancialDataView) ? await BuildFinancialKpisAsync(cancellationToken) : null;

        return new DashboardDto { PeriodFrom = from, PeriodTo = to, Operational = operational, Financial = financial };
    }

    public async Task<OperationalReportDto> GetOperationalReportAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var (from, to) = ResolvePeriod(filter);
        var entries = await BuildFilteredQuery(filter, from, to)
            .Select(t => new { t.ResourceId, t.ProjectId, t.OrderId, t.Resource.DepartmentId, t.Resource.ServiceId, t.Resource.TeamId, t.DureeHeures })
            .ToListAsync(cancellationToken);

        var departmentGroups = entries.GroupBy(e => e.DepartmentId).Select(g => (g.Key, g.Sum(e => e.DureeHeures))).ToList();
        var departmentIds = departmentGroups.Select(g => g.Key).ToList();
        var departmentNames = await departmentRepository.Query()
            .Where(d => departmentIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.Nom, cancellationToken);
        var chargeParDepartement = departmentGroups
            .Select(g => new OperationalReportGroupDto { Id = g.Key, Nom = departmentNames.GetValueOrDefault(g.Key, g.Key.ToString()), ChargeHeures = g.Item2 })
            .ToList();

        var serviceGroups = entries.GroupBy(e => e.ServiceId).Select(g => (g.Key, g.Sum(e => e.DureeHeures))).ToList();
        var serviceIds = serviceGroups.Select(g => g.Key).ToList();
        var serviceNames = await serviceRepository.Query()
            .Where(s => serviceIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Nom, cancellationToken);
        var chargeParService = serviceGroups
            .Select(g => new OperationalReportGroupDto { Id = g.Key, Nom = serviceNames.GetValueOrDefault(g.Key, g.Key.ToString()), ChargeHeures = g.Item2 })
            .ToList();

        var teamGroups = entries.Where(e => e.TeamId is not null).GroupBy(e => e.TeamId!.Value).Select(g => (g.Key, g.Sum(e => e.DureeHeures))).ToList();
        var teamIds = teamGroups.Select(g => g.Key).ToList();
        var teamNames = await teamRepository.Query()
            .Where(t => teamIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Nom, cancellationToken);
        var chargeParEquipe = teamGroups
            .Select(g => new OperationalReportGroupDto { Id = g.Key, Nom = teamNames.GetValueOrDefault(g.Key, g.Key.ToString()), ChargeHeures = g.Item2 })
            .ToList();

        var topProjets = await BuildTopProjectsAsync(entries.Select(e => (e.ProjectId, e.DureeHeures)), cancellationToken);
        var topCommandes = await BuildTopOrdersAsync(entries.Select(e => (e.OrderId, e.DureeHeures)), cancellationToken);

        var jalonsEnRetard = (await GetMilestonesEnRetardAsync(cancellationToken))
            .Select(m => new OperationalReportMilestoneDto { Id = m.Id, Nom = m.Nom, ProjectId = m.ProjectId, DatePrevue = m.DatePrevue })
            .ToList();

        var scopedResourceIds = await ResolveScopedResourceIdsAsync(filter, cancellationToken);
        var availabilities = await ResolveAvailabilitiesAsync(scopedResourceIds, from, to, cancellationToken);
        var (surcharges, sousCharges) = await BuildWorkloadAlertsAsync(
            scopedResourceIds, availabilities, entries.Select(e => (e.ResourceId, e.DureeHeures)), cancellationToken);

        var capacite = new List<OperationalReportCapacityDto>();
        foreach (var resourceId in scopedResourceIds)
        {
            if (!availabilities.TryGetValue(resourceId, out var availability))
            {
                continue;
            }
            capacite.Add(new OperationalReportCapacityDto
            {
                ResourceId = resourceId,
                Nom = await GetResourceNameAsync(resourceId, cancellationToken),
                CapaciteTheorique = availability.CapaciteTheorique,
                CapaciteReelle = availability.CapaciteReelle,
                TauxDisponibilite = availability.TauxDisponibilite
            });
        }

        return new OperationalReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            ChargeParDepartement = chargeParDepartement,
            ChargeParService = chargeParService,
            ChargeParEquipe = chargeParEquipe,
            ConsommationParProjet = topProjets,
            ConsommationParCommande = topCommandes,
            JalonsEnRetard = jalonsEnRetard,
            RessourcesSurchargees = surcharges,
            RessourcesSousUtilisees = sousCharges,
            CapaciteEtDisponibilite = capacite
        };
    }

    /// <summary>Ressource intégralement financière (§26.2) : null si l'appelant n'a pas
    /// FINANCIAL_DATA_VIEW — à traduire en 403 côté contrôleur (RequirePermissionAttribute).</summary>
    public async Task<FinancialReportDto?> GetFinancialReportAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        if (!currentUser.HasPermission(PermissionCodes.FinancialDataView))
        {
            return null;
        }

        var (from, to) = ResolvePeriod(filter);

        var snapshots = await snapshotRepository.Query()
            .Where(s => s.TimeEntry.Date >= from && s.TimeEntry.Date <= to && s.TimeEntry.Statut == ReferentialStatus.Actif)
            .Select(s => new
            {
                s.TimeEntry.Date,
                s.TimeEntry.ProjectId,
                s.TimeEntry.OrderId,
                s.TimeEntry.ResourceId,
                s.CompanyIdSnapshot,
                s.SourceTjmPersonne,
                s.SourceContrat,
                CoutReel = s.CoutReelCalcule ?? 0,
                CoutContrat = s.CoutContratCalcule ?? 0,
                Differentiel = s.DifferentielCalcule ?? 0
            })
            .ToListAsync(cancellationToken);

        var differentielGlobal = snapshots.Sum(s => s.Differentiel);
        var coutReelTotal = snapshots.Sum(s => s.CoutReel);

        var projectGroups = snapshots.Where(s => s.ProjectId is not null)
            .GroupBy(s => s.ProjectId!.Value)
            .Select(g => (Id: g.Key, CoutReel: g.Sum(s => s.CoutReel), CoutContrat: g.Sum(s => s.CoutContrat), Differentiel: g.Sum(s => s.Differentiel)))
            .ToList();
        var projectNames = await projectRepository.Query()
            .Where(p => projectGroups.Select(g => g.Id).Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Nom, cancellationToken);
        var parProjet = BuildFinancialDifferentials(projectGroups, projectNames);

        var orderGroups = snapshots.Where(s => s.OrderId is not null)
            .GroupBy(s => s.OrderId!.Value)
            .Select(g => (Id: g.Key, CoutReel: g.Sum(s => s.CoutReel), CoutContrat: g.Sum(s => s.CoutContrat), Differentiel: g.Sum(s => s.Differentiel)))
            .ToList();
        var orderNames = await orderRepository.Query()
            .Where(o => orderGroups.Select(g => g.Id).Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Reference, cancellationToken);
        var parCommande = BuildFinancialDifferentials(orderGroups, orderNames);

        var companyGroups = snapshots.Where(s => s.CompanyIdSnapshot is not null)
            .GroupBy(s => s.CompanyIdSnapshot!.Value)
            .Select(g => (Id: g.Key, CoutReel: g.Sum(s => s.CoutReel), CoutContrat: g.Sum(s => s.CoutContrat), Differentiel: g.Sum(s => s.Differentiel)))
            .ToList();
        var companyNames = await companyRepository.Query()
            .Where(c => companyGroups.Select(g => g.Id).Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Nom, cancellationToken);
        var parSociete = BuildFinancialDifferentials(companyGroups, companyNames);

        var resourceGroups = snapshots
            .GroupBy(s => s.ResourceId)
            .Select(g => (Id: g.Key, CoutReel: g.Sum(s => s.CoutReel), CoutContrat: g.Sum(s => s.CoutContrat), Differentiel: g.Sum(s => s.Differentiel)))
            .ToList();
        var resourceNames = await resourceRepository.Query()
            .Where(r => resourceGroups.Select(g => g.Id).Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => $"{r.Prenom} {r.Nom}", cancellationToken);
        var parRessource = BuildFinancialDifferentials(resourceGroups, resourceNames);

        var parMois = snapshots
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .Select(g => new FinancialReportMonthlyConsumptionDto
            {
                Annee = g.Key.Year,
                Mois = g.Key.Month,
                CoutReel = g.Sum(s => s.CoutReel),
                CoutContractuel = g.Sum(s => s.CoutContrat),
                Differentiel = g.Sum(s => s.Differentiel)
            })
            .OrderBy(m => m.Annee).ThenBy(m => m.Mois)
            .ToList();

        var budgetAjusteTotal = await budgetRepository.Query().SumAsync(b => (decimal?)b.AdjustedAmount, cancellationToken) ?? 0m;
        var besoinsRallonge = await BuildOrderAlertsAsync(a => a.RestFinancier < 0, cancellationToken);
        var renewalThreshold = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        var commandesARenouveler = await BuildOrderAlertsAsync(
            a => a.DateFinAjustee is not null && a.DateFinAjustee <= renewalThreshold && (a.StatusCode == "ACTIVE" || a.StatusCode == "SUSPENDUE"), cancellationToken);

        var sources = snapshots
            .SelectMany(s => new[] { s.SourceTjmPersonne, s.SourceContrat })
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => s!)
            .Distinct()
            .ToList();

        return new FinancialReportDto
        {
            PeriodFrom = from,
            PeriodTo = to,
            DifferentielGlobal = differentielGlobal,
            BudgetRestant = budgetAjusteTotal - coutReelTotal,
            AtterrissageEstime = coutReelTotal,
            DifferentielParProjet = parProjet,
            DifferentielParCommande = parCommande,
            DifferentielParSociete = parSociete,
            DifferentielParRessource = parRessource,
            ConsommationMensuelle = parMois,
            BesoinsRallonge = besoinsRallonge,
            CommandesARenouveler = commandesARenouveler,
            SourcesMontants = sources
        };
    }

    /// <summary>Cahier des charges §17.7 : ferme l'écart identifié à la clôture du Lot 4 (aucune
    /// synthèse des références opérationnelles liées à un projet n'existait alors).</summary>
    public async Task<IReadOnlyList<ProjectLinkedReferenceDto>> GetProjectLinkedReferencesAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var entries = await timeEntryRepository.Query()
            .Where(t => t.ProjectId == projectId && t.Statut == ReferentialStatus.Actif && t.Reference != null)
            .Select(t => new { t.Reference, t.ActivityTypeId, t.Date, t.DureeHeures })
            .ToListAsync(cancellationToken);

        var activityTypes = await LoadActivityTypesAsync(entries.Select(e => e.ActivityTypeId), cancellationToken);

        return entries
            .GroupBy(e => e.Reference)
            .Select(g =>
            {
                var activityType = activityTypes.GetValueOrDefault(g.First().ActivityTypeId);
                return new ProjectLinkedReferenceDto
                {
                    Reference = g.Key!,
                    ActivityTypeCode = activityType?.Code ?? string.Empty,
                    ActivityTypeLibelle = activityType?.Libelle ?? string.Empty,
                    NombreSaisies = g.Count(),
                    ChargeHeures = g.Sum(e => e.DureeHeures),
                    PremiereDate = g.Min(e => e.Date),
                    DerniereDate = g.Max(e => e.Date)
                };
            })
            .OrderByDescending(r => r.DerniereDate)
            .ToList();
    }

    public async Task<ReportingTableDto> GetChargesTableAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var report = await GetChargesReportAsync(filter, cancellationToken);
        var rows = new List<string[]>();
        rows.AddRange(report.TopApplications.Select(e => new[] { "Application", e.Nom, e.ChargeHeures.ToString("0.##") }));
        rows.AddRange(report.TopUtilisateurs.Select(e => new[] { "Utilisateur", e.Nom, e.ChargeHeures.ToString("0.##") }));
        rows.AddRange(report.TopProjets.Select(e => new[] { "Projet", e.Nom, e.ChargeHeures.ToString("0.##") }));
        rows.AddRange(report.TopCommandes.Select(e => new[] { "Commande", e.Nom, e.ChargeHeures.ToString("0.##") }));

        return new ReportingTableDto
        {
            Title = $"Charges du {report.PeriodFrom:yyyy-MM-dd} au {report.PeriodTo:yyyy-MM-dd}",
            Columns = ["Type", "Nom", "Charge (heures)"],
            Rows = rows
        };
    }

    public async Task<ReportingTableDto?> GetFinancialDifferentialsTableAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var report = await GetFinancialReportAsync(filter, cancellationToken);
        if (report is null)
        {
            return null;
        }

        var rows = new List<string[]>();
        void AddRows(string dimension, IReadOnlyList<FinancialReportDifferentialDto> source) =>
            rows.AddRange(source.Select(e => new[] { dimension, e.Nom, e.CoutReel.ToString("0.##"), e.CoutContractuel.ToString("0.##"), e.Differentiel.ToString("0.##") }));

        AddRows("Projet", report.DifferentielParProjet);
        AddRows("Commande", report.DifferentielParCommande);
        AddRows("Société", report.DifferentielParSociete);
        AddRows("Ressource", report.DifferentielParRessource);

        return new ReportingTableDto
        {
            Title = $"Différentiels du {report.PeriodFrom:yyyy-MM-dd} au {report.PeriodTo:yyyy-MM-dd}",
            Columns = ["Dimension", "Nom", "Coût réel", "Coût contractuel", "Différentiel"],
            Rows = rows
        };
    }

    /// <summary>§26.1/§26.3 (Lot 12, décision 2) : contenu distinct de Charges (§21) — charge par
    /// équipe/service/département, jalons en retard, capacité et disponibilité — même moteur
    /// d'export générique que GetChargesTableAsync/GetFinancialDifferentialsTableAsync (Lot 5),
    /// aucune nouvelle logique de calcul.</summary>
    public async Task<ReportingTableDto> GetOperationalTableAsync(ReportingFilterQuery filter, CancellationToken cancellationToken = default)
    {
        var report = await GetOperationalReportAsync(filter, cancellationToken);
        var rows = new List<string[]>();
        rows.AddRange(report.ChargeParEquipe.Select(e => new[] { "Équipe", e.Nom, e.ChargeHeures.ToString("0.##"), "" }));
        rows.AddRange(report.ChargeParService.Select(e => new[] { "Service", e.Nom, e.ChargeHeures.ToString("0.##"), "" }));
        rows.AddRange(report.ChargeParDepartement.Select(e => new[] { "Département", e.Nom, e.ChargeHeures.ToString("0.##"), "" }));
        rows.AddRange(report.ConsommationParProjet.Select(e => new[] { "Projet", e.Nom, e.ChargeHeures.ToString("0.##"), "" }));
        rows.AddRange(report.ConsommationParCommande.Select(e => new[] { "Commande", e.Nom, e.ChargeHeures.ToString("0.##"), "" }));
        rows.AddRange(report.JalonsEnRetard.Select(m => new[] { "Jalon en retard", m.Nom, m.DatePrevue.ToString("yyyy-MM-dd"), "" }));
        rows.AddRange(report.RessourcesSurchargees.Select(r => new[] { "Ressource surchargée", r.Nom, r.ChargeHeures.ToString("0.##"), r.CapaciteReelle.ToString("0.##") }));
        rows.AddRange(report.RessourcesSousUtilisees.Select(r => new[] { "Ressource sous-utilisée", r.Nom, r.ChargeHeures.ToString("0.##"), r.CapaciteReelle.ToString("0.##") }));
        rows.AddRange(report.CapaciteEtDisponibilite.Select(c => new[] { "Capacité", c.Nom, c.CapaciteReelle.ToString("0.##"), $"{c.TauxDisponibilite:0.##}%" }));

        return new ReportingTableDto
        {
            Title = $"Rapport opérationnel du {report.PeriodFrom:yyyy-MM-dd} au {report.PeriodTo:yyyy-MM-dd}",
            Columns = ["Dimension", "Nom", "Valeur", "Détail"],
            Rows = rows
        };
    }

    /// <summary>Factorise les 4 ventilations différentielles de GetFinancialReportAsync (projet,
    /// commande, société, ressource, sous-lot 14.6 de l'audit du Lot 14) : même agrégat
    /// (CoutReel/CoutContrat/Differentiel déjà sommés par dimension), même résolution de nom avec
    /// repli sur l'identifiant, même tri par |différentiel| décroissant — aucune règle de calcul
    /// nouvelle, uniquement la mise en commun d'un bloc jusque-là dupliqué 4 fois à l'identique.</summary>
    private static List<FinancialReportDifferentialDto> BuildFinancialDifferentials(
        IEnumerable<(Guid Id, decimal CoutReel, decimal CoutContrat, decimal Differentiel)> groups,
        IReadOnlyDictionary<Guid, string> names) =>
        groups
            .Select(g => new FinancialReportDifferentialDto
            {
                Id = g.Id,
                Nom = names.GetValueOrDefault(g.Id, g.Id.ToString()),
                CoutReel = g.CoutReel,
                CoutContractuel = g.CoutContrat,
                Differentiel = g.Differentiel
            })
            .OrderByDescending(d => Math.Abs(d.Differentiel))
            .ToList();

    private static (DateOnly From, DateOnly To) ResolvePeriod(ReportingFilterQuery filter) =>
        ReportingPeriodResolver.Resolve(filter.PeriodType, filter.ReferenceDate, filter.CustomFrom, filter.CustomTo);

    private IQueryable<TimeEntry> BuildFilteredQuery(ReportingFilterQuery filter, DateOnly from, DateOnly to)
    {
        var query = timeEntryRepository.Query().Where(t => t.Statut == ReferentialStatus.Actif && t.Date >= from && t.Date <= to);

        if (filter.ProjectId is not null) query = query.Where(t => t.ProjectId == filter.ProjectId);
        if (filter.OrderId is not null) query = query.Where(t => t.OrderId == filter.OrderId);
        if (filter.ResourceId is not null) query = query.Where(t => t.ResourceId == filter.ResourceId);
        if (filter.ActivityTypeId is not null) query = query.Where(t => t.ActivityTypeId == filter.ActivityTypeId);
        // Une application n'est reliée qu'au travers d'un projet (aucun lien direct TimeEntry -> ApplicationReference).
        if (filter.ApplicationId is not null) query = query.Where(t => t.Project != null && t.Project.ApplicationId == filter.ApplicationId);
        if (filter.DepartmentId is not null) query = query.Where(t => t.Resource.DepartmentId == filter.DepartmentId);
        if (filter.ServiceId is not null) query = query.Where(t => t.Resource.ServiceId == filter.ServiceId);
        if (filter.TeamId is not null) query = query.Where(t => t.Resource.TeamId == filter.TeamId);
        if (filter.OperationalRoleId is not null) query = query.Where(t => t.Resource.OperationalRoles.Any(r => r.OperationalRoleId == filter.OperationalRoleId));

        return query;
    }

    private async Task<List<Guid>> ResolveScopedResourceIdsAsync(ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var query = resourceRepository.Query().Where(r => r.Statut == ReferentialStatus.Actif);
        if (filter.ResourceId is not null) query = query.Where(r => r.Id == filter.ResourceId);
        if (filter.DepartmentId is not null) query = query.Where(r => r.DepartmentId == filter.DepartmentId);
        if (filter.ServiceId is not null) query = query.Where(r => r.ServiceId == filter.ServiceId);
        if (filter.TeamId is not null) query = query.Where(r => r.TeamId == filter.TeamId);
        if (filter.OperationalRoleId is not null) query = query.Where(r => r.OperationalRoles.Any(o => o.OperationalRoleId == filter.OperationalRoleId));

        return await query.Select(r => r.Id).ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, ActivityType>> LoadActivityTypesAsync(IEnumerable<Guid> activityTypeIds, CancellationToken cancellationToken)
    {
        var ids = activityTypeIds.Distinct().ToList();
        return await activityTypeRepository.Query().Where(a => ids.Contains(a.Id)).ToDictionaryAsync(a => a.Id, cancellationToken);
    }

    private static int CountDistinctReferences(
        IEnumerable<(Guid ActivityTypeId, string? Reference)> entries, Dictionary<Guid, ActivityType> activityTypes, string code) =>
        entries
            .Where(e => activityTypes.GetValueOrDefault(e.ActivityTypeId)?.Code == code && !string.IsNullOrEmpty(e.Reference))
            .Select(e => e.Reference!)
            .Distinct()
            .Count();

    private async Task<string> GetResourceNameAsync(Guid resourceId, CancellationToken cancellationToken)
    {
        var resource = await resourceRepository.Query().Where(r => r.Id == resourceId).Select(r => new { r.Nom, r.Prenom }).FirstAsync(cancellationToken);
        return $"{resource.Prenom} {resource.Nom}";
    }

    /// <summary>Résout la disponibilité de chaque ressource du périmètre une seule fois par appel
    /// public (sous-lot 14.6 de l'audit du Lot 14) : GetChargesReportAsync, GetDashboardAsync et
    /// GetOperationalReportAsync bouclaient chacun indépendamment sur AvailabilityService pour le
    /// même ensemble de ressources et la même période — GetDashboardAsync et
    /// GetOperationalReportAsync appellent en plus BuildWorkloadAlertsAsync, qui refaisait la même
    /// boucle une seconde fois. Résolu ici une seule fois puis partagé ; la formule de disponibilité
    /// elle-même (AvailabilityService) n'est pas modifiée.</summary>
    private async Task<Dictionary<Guid, AvailabilityResultDto>> ResolveAvailabilitiesAsync(
        IEnumerable<Guid> resourceIds, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, AvailabilityResultDto>();
        foreach (var resourceId in resourceIds)
        {
            var availability = await availabilityService.GetAvailabilityAsync(resourceId, from, to, cancellationToken);
            if (availability is not null)
            {
                result[resourceId] = availability;
            }
        }
        return result;
    }

    private async Task<(List<ChargesResourceAlertDto> Surcharges, List<ChargesResourceAlertDto> SousCharges)> BuildWorkloadAlertsAsync(
        IReadOnlyList<Guid> scopedResourceIds,
        IReadOnlyDictionary<Guid, AvailabilityResultDto> availabilities,
        IEnumerable<(Guid ResourceId, decimal DureeHeures)> filteredEntries,
        CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.Query().Select(s => new { s.SeuilSurcharge, s.SeuilSousCharge }).FirstAsync(cancellationToken);
        var chargeByResource = filteredEntries.GroupBy(e => e.ResourceId).ToDictionary(g => g.Key, g => g.Sum(e => e.DureeHeures));

        var surcharges = new List<ChargesResourceAlertDto>();
        var sousCharges = new List<ChargesResourceAlertDto>();

        foreach (var resourceId in scopedResourceIds)
        {
            if (!availabilities.TryGetValue(resourceId, out var availability) || availability.CapaciteReelle <= 0)
            {
                continue;
            }

            var charge = chargeByResource.GetValueOrDefault(resourceId);
            var dto = new ChargesResourceAlertDto
            {
                ResourceId = resourceId,
                Nom = await GetResourceNameAsync(resourceId, cancellationToken),
                ChargeHeures = charge,
                CapaciteReelle = availability.CapaciteReelle
            };

            if (settings.SeuilSurcharge is not null && charge > availability.CapaciteReelle * settings.SeuilSurcharge.Value / 100)
            {
                surcharges.Add(dto);
            }
            else if (settings.SeuilSousCharge is not null && charge < availability.CapaciteReelle * settings.SeuilSousCharge.Value / 100)
            {
                sousCharges.Add(dto);
            }
        }

        return (surcharges, sousCharges);
    }

    /// <summary>Lundi de la semaine contenant <paramref name="date"/> — même formule que
    /// ReportingPeriodResolver (jamais dupliquée avec un calcul différent).</summary>
    private static DateOnly GetWeekStart(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }

    /// <summary>§21.2/§21.3/§25.3 "prévu vs réalisé" (Lot 12, décision 3, docs/BACKLOG_METIER.md §16) :
    /// agrégation dédiée sur ProjectWeeklyPlan, jamais recalculée à partir d'une liste paginée
    /// (GET /project-planning, Lot 10, reste la vue détaillée par projet/ressource/semaine). Null si
    /// le filtre porte sur une dimension que la planification ne connaît pas (commande, type
    /// d'activité) : la charge planifiée n'est jamais approchée à zéro dans ce cas.</summary>
    private async Task<ChargesPlanComparisonDto> BuildPlanComparisonAsync(
        ReportingFilterQuery filter, DateOnly from, DateOnly to, decimal chargeRealisee, CancellationToken cancellationToken)
    {
        if (filter.OrderId is not null || filter.ActivityTypeId is not null)
        {
            return new ChargesPlanComparisonDto { ChargePrevue = null, ChargeRealisee = chargeRealisee };
        }

        var query = weeklyPlanRepository.Query()
            .Where(w => w.ProjectPlanVersion.Statut == ProjectPlanVersionStatus.Active
                && w.WeekStartDate >= from && w.WeekStartDate <= to);

        if (filter.ProjectId is not null) query = query.Where(w => w.ProjectPlanVersion.ProjectId == filter.ProjectId);
        if (filter.ApplicationId is not null) query = query.Where(w => w.ProjectPlanVersion.Project.ApplicationId == filter.ApplicationId);
        if (filter.ResourceId is not null) query = query.Where(w => w.ResourceId == filter.ResourceId);
        if (filter.DepartmentId is not null) query = query.Where(w => w.Resource.DepartmentId == filter.DepartmentId);
        if (filter.ServiceId is not null) query = query.Where(w => w.Resource.ServiceId == filter.ServiceId);
        if (filter.TeamId is not null) query = query.Where(w => w.Resource.TeamId == filter.TeamId);
        if (filter.OperationalRoleId is not null) query = query.Where(w => w.Resource.OperationalRoles.Any(o => o.OperationalRoleId == filter.OperationalRoleId));

        var rawLines = await query
            .Select(w => new { ProjectId = w.ProjectPlanVersion.ProjectId, w.ResourceId, w.WeekStartDate, w.ProjectPlanVersion.Type, w.ChargePlanifieeHeures })
            .ToListAsync(cancellationToken);

        // Initiale et Ajustée (Active) peuvent coexister à la même clé (Projet, Ressource, Semaine) —
        // même regroupement que ProjectPlanningService.GetOverviewAsync (Lot 10), jamais dupliqué avec
        // une règle différente : Ajustée si présente, sinon Initiale.
        var chargePrevue = rawLines
            .GroupBy(l => (l.ProjectId, l.ResourceId, l.WeekStartDate))
            .Sum(g => g.Any(l => l.Type == ProjectPlanVersionType.Ajuste)
                ? g.Where(l => l.Type == ProjectPlanVersionType.Ajuste).Sum(l => l.ChargePlanifieeHeures)
                : g.Where(l => l.Type == ProjectPlanVersionType.Initial).Sum(l => l.ChargePlanifieeHeures));

        return new ChargesPlanComparisonDto { ChargePrevue = chargePrevue, ChargeRealisee = chargeRealisee };
    }

    private async Task<List<ChargesTopEntryDto>> BuildTopApplicationsAsync(IEnumerable<(Guid? ProjectId, decimal Heures)> entries, CancellationToken cancellationToken)
    {
        var projectIds = entries.Where(e => e.ProjectId is not null).Select(e => e.ProjectId!.Value).Distinct().ToList();
        if (projectIds.Count == 0)
        {
            return [];
        }

        var projectApplications = await projectRepository.Query()
            .Where(p => projectIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ApplicationId, ApplicationNom = p.Application.Nom })
            .ToListAsync(cancellationToken);
        var applicationByProject = projectApplications.ToDictionary(p => p.Id, p => (p.ApplicationId, p.ApplicationNom));

        return entries
            .Where(e => e.ProjectId is not null && applicationByProject.ContainsKey(e.ProjectId.Value))
            .GroupBy(e => applicationByProject[e.ProjectId!.Value].ApplicationId)
            .Select(g => new ChargesTopEntryDto
            {
                Id = g.Key,
                Nom = applicationByProject.Values.First(a => a.ApplicationId == g.Key).ApplicationNom,
                ChargeHeures = g.Sum(e => e.Heures)
            })
            .OrderByDescending(e => e.ChargeHeures)
            .Take(5)
            .ToList();
    }

    private async Task<List<ChargesTopEntryDto>> BuildTopProjectsAsync(IEnumerable<(Guid? ProjectId, decimal Heures)> entries, CancellationToken cancellationToken)
    {
        var grouped = entries.Where(e => e.ProjectId is not null)
            .GroupBy(e => e.ProjectId!.Value)
            .Select(g => (Id: g.Key, Heures: g.Sum(e => e.Heures)))
            .OrderByDescending(g => g.Heures)
            .Take(5)
            .ToList();
        if (grouped.Count == 0)
        {
            return [];
        }

        var ids = grouped.Select(g => g.Id).ToList();
        var names = await projectRepository.Query().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Nom, cancellationToken);
        return grouped.Select(g => new ChargesTopEntryDto { Id = g.Id, Nom = names.GetValueOrDefault(g.Id, g.Id.ToString()), ChargeHeures = g.Heures }).ToList();
    }

    private async Task<List<ChargesTopEntryDto>> BuildTopOrdersAsync(IEnumerable<(Guid? OrderId, decimal Heures)> entries, CancellationToken cancellationToken)
    {
        var grouped = entries.Where(e => e.OrderId is not null)
            .GroupBy(e => e.OrderId!.Value)
            .Select(g => (Id: g.Key, Heures: g.Sum(e => e.Heures)))
            .OrderByDescending(g => g.Heures)
            .Take(5)
            .ToList();
        if (grouped.Count == 0)
        {
            return [];
        }

        var ids = grouped.Select(g => g.Id).ToList();
        var names = await orderRepository.Query().Where(o => ids.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Reference, cancellationToken);
        return grouped.Select(g => new ChargesTopEntryDto { Id = g.Id, Nom = names.GetValueOrDefault(g.Id, g.Id.ToString()), ChargeHeures = g.Heures }).ToList();
    }

    private async Task<List<ChargesTopEntryDto>> BuildTopResourcesAsync(IEnumerable<(Guid ResourceId, decimal Heures)> entries, CancellationToken cancellationToken)
    {
        var grouped = entries
            .GroupBy(e => e.ResourceId)
            .Select(g => (Id: g.Key, Heures: g.Sum(e => e.Heures)))
            .OrderByDescending(g => g.Heures)
            .Take(5)
            .ToList();
        if (grouped.Count == 0)
        {
            return [];
        }

        var ids = grouped.Select(g => g.Id).ToList();
        var names = await resourceRepository.Query().Where(r => ids.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => $"{r.Prenom} {r.Nom}", cancellationToken);
        return grouped.Select(g => new ChargesTopEntryDto { Id = g.Id, Nom = names.GetValueOrDefault(g.Id, g.Id.ToString()), ChargeHeures = g.Heures }).ToList();
    }

    /// <summary>Même règle "en retard" que MilestoneService.ToDto (§24.2), reformulée ici en
    /// prédicat SQL plutôt qu'en filtre mémoire après coup, pour éviter de charger tous les jalons
    /// de la base sur un tableau de bord (CLAUDE.md §5 : condition volontairement identique, pas
    /// une nouvelle règle).</summary>
    private async Task<List<Milestone>> GetMilestonesEnRetardAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await milestoneRepository.Query()
            .Where(m => m.DatePrevue < today && m.Statut != MilestoneStatus.Termine && m.Statut != MilestoneStatus.Annule)
            .ToListAsync(cancellationToken);
    }

    private async Task<DashboardFinancialKpisDto> BuildFinancialKpisAsync(CancellationToken cancellationToken)
    {
        var budgetTotals = await budgetRepository.Query()
            .GroupBy(_ => 1)
            .Select(g => new { Initial = g.Sum(b => b.InitialAmount), Adjusted = g.Sum(b => b.AdjustedAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        var snapshotTotals = await snapshotRepository.Query()
            .GroupBy(_ => 1)
            .Select(g => new { CoutReel = g.Sum(s => s.CoutReelCalcule ?? 0), CoutContrat = g.Sum(s => s.CoutContratCalcule ?? 0), Differentiel = g.Sum(s => s.DifferentielCalcule ?? 0) })
            .FirstOrDefaultAsync(cancellationToken);

        var coutReelTotal = snapshotTotals?.CoutReel ?? 0m;
        var budgetAjusteTotal = budgetTotals?.Adjusted ?? 0m;

        var projetsSousFinances = 0;
        var projects = await projectRepository.Query().Where(p => p.BudgetInitial != null).Select(p => new { p.Id, p.BudgetInitial }).ToListAsync(cancellationToken);
        foreach (var project in projects)
        {
            var budgetAjusteProjet = await budgetRepository.Query().Where(b => b.ProjectId == project.Id).Select(b => (decimal?)b.AdjustedAmount).FirstOrDefaultAsync(cancellationToken)
                ?? project.BudgetInitial!.Value;
            var coutReelProjet = await snapshotRepository.Query().Where(s => s.TimeEntry.ProjectId == project.Id).SumAsync(s => s.CoutReelCalcule ?? 0, cancellationToken);
            if (ProjectPlanningCalculator.CalculateBudgetRisk(coutReelProjet, budgetAjusteProjet))
            {
                projetsSousFinances++;
            }
        }

        var commandesARisque = (await BuildOrderAlertsAsync(a => a.RestFinancier < 0, cancellationToken)).Count;

        return new DashboardFinancialKpisDto
        {
            BudgetInitialTotal = budgetTotals?.Initial ?? 0m,
            BudgetAjusteTotal = budgetAjusteTotal,
            CoutReelTotal = coutReelTotal,
            CoutContractuelTotal = snapshotTotals?.CoutContrat ?? 0m,
            DifferentielGlobal = snapshotTotals?.Differentiel ?? 0m,
            BudgetRestant = budgetAjusteTotal - coutReelTotal,
            CommandesARisque = commandesARisque,
            ProjetsSousFinances = projetsSousFinances,
            AtterrissageEstime = coutReelTotal
        };
    }

    private async Task<List<FinancialReportOrderAlertDto>> BuildOrderAlertsAsync(
        Func<(decimal RestFinancier, DateOnly? DateFinAjustee, string StatusCode), bool> predicate, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.Query()
            .Select(o => new { o.Id, o.Reference, o.BudgetFinancierAjuste, o.DateFinAjustee, StatusCode = o.Status.Code })
            .ToListAsync(cancellationToken);

        var result = new List<FinancialReportOrderAlertDto>();
        foreach (var order in orders)
        {
            var coutReel = await snapshotRepository.Query().Where(s => s.TimeEntry.OrderId == order.Id).SumAsync(s => s.CoutReelCalcule ?? 0, cancellationToken);
            var restFinancier = order.BudgetFinancierAjuste - coutReel;
            if (predicate((restFinancier, order.DateFinAjustee, order.StatusCode)))
            {
                result.Add(new FinancialReportOrderAlertDto
                {
                    OrderId = order.Id,
                    Reference = order.Reference,
                    BudgetFinancierAjuste = order.BudgetFinancierAjuste,
                    CoutReelConsomme = coutReel,
                    DateFinAjustee = order.DateFinAjustee
                });
            }
        }

        return result;
    }
}
