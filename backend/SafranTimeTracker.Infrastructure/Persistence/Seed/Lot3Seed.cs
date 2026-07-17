using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Absences;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Time;
using HolidayCalendarEntity = SafranTimeTracker.Domain.Settings.HolidayCalendar;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 3 : les 13 types d'activité (§19.2) classés RUN/hors RUN
/// (§29.4) avec leurs métadonnées de validation de référence (§19.3), quelques jours fériés, une
/// variation de capacité, des saisies de temps couvrant les cas notables (société interne, société
/// externe avec différentiel positif, historisation TJM dans le temps, valorisation incomplète —
/// docs/DATABASE.md §7), et des absences dans différents statuts du workflow (§23.3). Idempotent
/// (HasData), dates/montants strictement déterministes (toutes les saisies utilisent 7,75 heures,
/// soit exactement 1 jour, pour des calculs financiers lisibles).
/// </summary>
internal static class Lot3Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";
    private const decimal UneJourneeHeures = 7.75m;

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedActivityTypes(modelBuilder);
        SeedHolidays(modelBuilder);
        SeedCapacityPeriod(modelBuilder);
        SeedInactiveResource(modelBuilder);
        SeedTimeEntries(modelBuilder);
        SeedAbsences(modelBuilder);
    }

    /// <summary>Ressource dédiée à la démonstration/aux tests du blocage "saisie sur ressource
    /// inactive" (§19.4) : aucune saisie de temps n'existe volontairement pour elle.</summary>
    private static void SeedInactiveResource(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>().HasData(new Resource
        {
            Id = SeedIds.ResourceInactiveDemo,
            Nom = "RESSOURCE-INACTIVE",
            Prenom = "Démonstration",
            DepartmentId = SeedIds.DepartmentDsi,
            ServiceId = SeedIds.ServiceProductionApplicative,
            DailyCapacity = 7.75m,
            WeeklyCapacity = 38.75m,
            Statut = ReferentialStatus.Inactif,
            Commentaire = "Ressource désactivée de démonstration (test de blocage §19.4).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedActivityTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityType>().HasData(
            ActivityType(SeedIds.ActivityTypeRun, "RUN", "RUN", isRun: true, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeIncident, "INCIDENT", "Incident", isRun: true, referenceRequired: true, @"^INC\d{7}$", "INC0001234"),
            ActivityType(SeedIds.ActivityTypeChange, "CHANGE", "Change", isRun: true, referenceRequired: true, @"^CHG\d{7}$", "CHG0001234"),
            ActivityType(SeedIds.ActivityTypeProblem, "PROBLEM", "Problem", isRun: true, referenceRequired: true, @"^PRB\d{7}$", "PRB0001234"),
            ActivityType(SeedIds.ActivityTypeRitm, "RITM", "RITM", isRun: true, referenceRequired: true, @"^RITM\d{7}$", "RITM0001234"),
            ActivityType(SeedIds.ActivityTypeProjet, "PROJET", "Projet", isRun: false, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeAmeliorationContinue, "AMELIORATION_CONTINUE", "Amélioration continue", isRun: false, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeSupport, "SUPPORT", "Support", isRun: true, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeAstreinte, "ASTREINTE", "Astreinte", isRun: true, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeReunion, "REUNION", "Réunion", isRun: false, referenceRequired: false),
            ActivityType(SeedIds.ActivityTypeFormation, "FORMATION", "Formation", isRun: false, referenceRequired: false),
            // IsRun corrigé à true au Lot 6 (règle métier validée : "les références INC/CHG/PRB/
            // RITM/VABE/VSR sont des références RUN") — écart de seed du Lot 3 (VABE/VSR étaient
            // seedés hors RUN), voir docs/IMPLEMENTATION_STATUS.md.
            ActivityType(SeedIds.ActivityTypeVabe, "VABE", "VABE", isRun: true, referenceRequired: true, @"^VABE-\d{4}$", "VABE-0012"),
            ActivityType(SeedIds.ActivityTypeVsr, "VSR", "VSR", isRun: true, referenceRequired: true, @"^VSR-\d{4}$", "VSR-0012"));

        ActivityType ActivityType(
            Guid id, string code, string libelle, bool isRun, bool referenceRequired,
            string? referenceFormatRegex = null, string? referenceExample = null) => new()
        {
            Id = id,
            Code = code,
            Libelle = libelle,
            IsRun = isRun,
            ReferenceRequired = referenceRequired,
            ReferenceFormatRegex = referenceFormatRegex,
            ReferenceExample = referenceExample,
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedHolidays(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HolidayCalendarEntity>().HasData(
            Holiday(SeedIds.HolidayNouvelAn2024, new DateOnly(2024, 1, 1), "Jour de l'an"),
            Holiday(SeedIds.HolidayFeteTravail2024, new DateOnly(2024, 5, 1), "Fête du travail"),
            Holiday(SeedIds.HolidayFeteNationale2024, new DateOnly(2024, 7, 14), "Fête nationale"),
            Holiday(SeedIds.HolidayNoel2024, new DateOnly(2024, 12, 25), "Noël"),
            Holiday(SeedIds.HolidayNouvelAn2025, new DateOnly(2025, 1, 1), "Jour de l'an"));

        HolidayCalendarEntity Holiday(Guid id, DateOnly date, string libelle) => new()
        {
            Id = id,
            Date = date,
            Libelle = libelle,
            Pays = "France",
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedCapacityPeriod(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResourceCapacityPeriod>().HasData(new ResourceCapacityPeriod
        {
            Id = SeedIds.CapacityPeriodPatel,
            ResourceId = SeedIds.ResourcePatel,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = null,
            DailyCapacity = 4.00m,
            WeeklyCapacity = 20.00m,
            Reason = "Temps partiel (démonstration, cahier des charges §10.5).",
            Status = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedTimeEntries(ModelBuilder modelBuilder)
    {
        // BERNARD (société interne) : RUN puis Incident sur la commande de démonstration
        // (société compatible, §13.4) -> coût réel seul, contractuel/différentiel non applicables.
        // LEGRAND (société externe avec contrat) -> différentiel positif calculable (§20.4).
        // GEORGES : même type d'activité à un an d'écart -> démontre la recherche du TJM à la date
        // de la saisie au travers de l'historisation (§11.3), pas seulement dans le calcul isolé.
        // MISHRA : aucun TJM -> valorisation incomplète (§11.4), aucun montant inventé.
        modelBuilder.Entity<TimeEntry>().HasData(
            TimeEntry(SeedIds.TimeEntryBernardRun, SeedIds.ResourceBernard, SeedIds.ActivityTypeRun,
                new DateOnly(2024, 6, 10), orderId: null, reference: null),
            TimeEntry(SeedIds.TimeEntryBernardIncident, SeedIds.ResourceBernard, SeedIds.ActivityTypeIncident,
                new DateOnly(2024, 6, 11), orderId: SeedIds.OrderDemo, reference: "INC0001234"),
            // Rattachée à ProjectMigrationElm (Lot 4) : démontre l'agrégation charge/coût consommés
            // d'un projet à partir des saisies réelles (§17.5, §18.1, §20.6).
            TimeEntry(SeedIds.TimeEntryLegrandProjet, SeedIds.ResourceLegrand, SeedIds.ActivityTypeProjet,
                new DateOnly(2024, 6, 10), orderId: null, reference: null, projectId: SeedIds.ProjectMigrationElm),
            TimeEntry(SeedIds.TimeEntryGeorgesChange2024, SeedIds.ResourceGeorges, SeedIds.ActivityTypeChange,
                new DateOnly(2024, 3, 15), orderId: null, reference: "CHG0001234"),
            TimeEntry(SeedIds.TimeEntryGeorgesChange2025, SeedIds.ResourceGeorges, SeedIds.ActivityTypeChange,
                new DateOnly(2025, 3, 15), orderId: null, reference: "CHG0009999"),
            TimeEntry(SeedIds.TimeEntryMishraFormation, SeedIds.ResourceMishra, SeedIds.ActivityTypeFormation,
                new DateOnly(2024, 6, 10), orderId: null, reference: null));

        // Id partagé avec TimeEntry (Id == TimeEntryId, cf. TimeEntryService.ValorizeAsync) : pas
        // de Guid distinct par snapshot.
        modelBuilder.Entity<TimeEntryFinancialSnapshot>().HasData(
            Snapshot(SeedIds.TimeEntryBernardRun,
                tjmPersonne: 650.00m, resourceTjmHistoryId: SeedIds.TjmBernard,
                tjmContrat: null, contractHistoryId: null, companyId: SeedIds.CompanySafran,
                coutReel: 650.00m, coutContrat: null, differentiel: null, FinancialValuationStatus.Complete),
            Snapshot(SeedIds.TimeEntryBernardIncident,
                tjmPersonne: 650.00m, resourceTjmHistoryId: SeedIds.TjmBernard,
                tjmContrat: null, contractHistoryId: null, companyId: SeedIds.CompanySafran,
                coutReel: 650.00m, coutContrat: null, differentiel: null, FinancialValuationStatus.Complete),
            Snapshot(SeedIds.TimeEntryLegrandProjet,
                tjmPersonne: 700.00m, resourceTjmHistoryId: SeedIds.TjmLegrand,
                tjmContrat: 750.00m, contractHistoryId: SeedIds.ContractExterneConseil, companyId: SeedIds.CompanyExterneConseil,
                coutReel: 700.00m, coutContrat: 750.00m, differentiel: 50.00m, FinancialValuationStatus.Complete),
            Snapshot(SeedIds.TimeEntryGeorgesChange2024,
                tjmPersonne: 600.00m, resourceTjmHistoryId: SeedIds.TjmGeorges1,
                tjmContrat: null, contractHistoryId: null, companyId: SeedIds.CompanySafran,
                coutReel: 600.00m, coutContrat: null, differentiel: null, FinancialValuationStatus.Complete),
            Snapshot(SeedIds.TimeEntryGeorgesChange2025,
                tjmPersonne: 620.00m, resourceTjmHistoryId: SeedIds.TjmGeorges2,
                tjmContrat: null, contractHistoryId: null, companyId: SeedIds.CompanySafran,
                coutReel: 620.00m, coutContrat: null, differentiel: null, FinancialValuationStatus.Complete),
            Snapshot(SeedIds.TimeEntryMishraFormation,
                tjmPersonne: null, resourceTjmHistoryId: null,
                tjmContrat: null, contractHistoryId: null, companyId: null,
                coutReel: null, coutContrat: null, differentiel: null, FinancialValuationStatus.Incomplete));

        TimeEntry TimeEntry(
            Guid id, Guid resourceId, Guid activityTypeId, DateOnly date, Guid? orderId, string? reference, Guid? projectId = null) => new()
        {
            Id = id,
            ResourceId = resourceId,
            ActivityTypeId = activityTypeId,
            OrderId = orderId,
            ProjectId = projectId,
            Date = date,
            DureeHeures = UneJourneeHeures,
            Reference = reference,
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };

        TimeEntryFinancialSnapshot Snapshot(
            Guid timeEntryId, decimal? tjmPersonne, Guid? resourceTjmHistoryId,
            decimal? tjmContrat, Guid? contractHistoryId, Guid? companyId,
            decimal? coutReel, decimal? coutContrat, decimal? differentiel, FinancialValuationStatus status) => new()
        {
            Id = timeEntryId,
            TimeEntryId = timeEntryId,
            TjmPersonneSnapshot = tjmPersonne,
            SourceTjmPersonne = resourceTjmHistoryId is null ? null : "ResourceTjmHistory",
            ResourceTjmHistoryId = resourceTjmHistoryId,
            TjmContratSnapshot = tjmContrat,
            SourceContrat = contractHistoryId is null ? null : "CompanyContractHistory",
            CompanyContractHistoryId = contractHistoryId,
            CompanyIdSnapshot = companyId,
            CoutReelCalcule = coutReel,
            CoutContratCalcule = coutContrat,
            DifferentielCalcule = differentiel,
            CalculationDate = SeedTimestamp,
            CalculationStatus = status,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedAbsences(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Absence>().HasData(
            new Absence
            {
                Id = SeedIds.AbsenceBernardConge, ResourceId = SeedIds.ResourceBernard, Type = AbsenceType.Conge,
                DateDebut = new DateOnly(2024, 7, 1), DateFin = new DateOnly(2024, 7, 5), DemiJournee = false,
                Statut = AbsenceStatus.Valide, ValideParIdentifiant = "flegrand", DateDecision = SeedTimestamp,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            },
            new Absence
            {
                Id = SeedIds.AbsenceLegrandMaladie, ResourceId = SeedIds.ResourceLegrand, Type = AbsenceType.Maladie,
                DateDebut = new DateOnly(2024, 8, 12), DateFin = new DateOnly(2024, 8, 12), DemiJournee = true,
                Statut = AbsenceStatus.Soumis,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            },
            new Absence
            {
                Id = SeedIds.AbsenceGeorgesRtt, ResourceId = SeedIds.ResourceGeorges, Type = AbsenceType.Rtt,
                DateDebut = new DateOnly(2024, 9, 2), DateFin = new DateOnly(2024, 9, 2), DemiJournee = false,
                Statut = AbsenceStatus.Brouillon,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            },
            new Absence
            {
                Id = SeedIds.AbsenceMishraFormationRefusee, ResourceId = SeedIds.ResourceMishra, Type = AbsenceType.Formation,
                DateDebut = new DateOnly(2024, 5, 10), DateFin = new DateOnly(2024, 5, 10), DemiJournee = false,
                Statut = AbsenceStatus.Refuse, ValideParIdentifiant = "tgeorges", DateDecision = SeedTimestamp,
                Commentaire = "Formation déjà suivie récemment.",
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            });
    }
}
