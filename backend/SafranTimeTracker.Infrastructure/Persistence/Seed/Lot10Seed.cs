using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration enrichi pour le Lot 10 (§35 : "8 projets" et "15 jalons" au
/// minimum, alors que le jeu antérieur — Lot4Seed — n'en portait que 2 et 3). Uniquement des
/// données, aucune nouvelle règle métier (décision actée à l'ouverture du Lot 10) : les 6 projets
/// ajoutés ici réutilisent exclusivement les référentiels déjà seedés (applications, ressources,
/// départements/services/équipes, statuts, types de projet, clients). Variété volontaire sur les
/// axes de filtrage du §16.1 (statut, application, pilote, service, équipe, niveau de risque,
/// période, alerte planning) pour que les écrans du Lot 10 soient réellement démontrables.
/// Idempotent (HasData), dates/montants strictement déterministes.
/// </summary>
internal static class Lot10Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedProjects(modelBuilder);
        SeedParticipants(modelBuilder);
        SeedPlanningAndWeeklyPlans(modelBuilder);
        SeedMilestones(modelBuilder);
        SeedBudgets(modelBuilder);
    }

    private static void SeedProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = SeedIds.ProjectPortailRunServiceNow,
                Nom = "Portail RUN ServiceNow",
                Code = "PRJ-SNOW-2025",
                ApplicationId = SeedIds.AppServiceNow,
                DescriptionCourte = "Portail self-service RUN sur ServiceNow.",
                PiloteId = SeedIds.ResourceManceron,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceRunMco,
                TeamId = SeedIds.TeamRunA,
                StatusId = SeedIds.ProjectStatusActif,
                ProjectTypeId = SeedIds.ProjectTypeRegie,
                ClientId = SeedIds.ClientDirectionSupport,
                DateDebut = new DateOnly(2025, 1, 1),
                DateFinPrevueInitiale = new DateOnly(2025, 9, 30),
                BudgetInitial = 60000.00m,
                NiveauRisque = ProjectRiskLevel.Eleve,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectSupportServiceNowN2,
                Nom = "Support ServiceNow N2",
                Code = "PRJ-SNOW-SUP-2025",
                ApplicationId = SeedIds.AppServiceNow,
                DescriptionCourte = "Montée en compétence support niveau 2 sur ServiceNow.",
                PiloteId = SeedIds.ResourceFocquenoey,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceSupport,
                StatusId = SeedIds.ProjectStatusActif,
                DateDebut = new DateOnly(2025, 3, 1),
                DateFinPrevueInitiale = new DateOnly(2025, 12, 31),
                BudgetInitial = 25000.00m,
                NiveauRisque = ProjectRiskLevel.Faible,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectObservabiliteVtom,
                Nom = "Observabilité VTOM",
                Code = "PRJ-VTOM-OBS-2024",
                ApplicationId = SeedIds.AppVtom,
                DescriptionCourte = "Mise en place d'une supervision proactive de l'ordonnanceur.",
                PiloteId = SeedIds.ResourceReau,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceRunMco,
                TeamId = SeedIds.TeamRunA,
                StatusId = SeedIds.ProjectStatusSuspendu,
                DateDebut = new DateOnly(2024, 6, 1),
                DateFinPrevueInitiale = new DateOnly(2025, 2, 28),
                DateFinAjustee = new DateOnly(2025, 5, 31), // dérive planning -> risque planning (§29.5)
                BudgetInitial = 45000.00m,
                NiveauRisque = ProjectRiskLevel.Moyen,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectRefontePortailElm,
                Nom = "Refonte Portail ELM",
                Code = "PRJ-ELM-PORTAIL-2025",
                ApplicationId = SeedIds.AppIbmElm,
                DescriptionCourte = "Refonte du portail utilisateur IBM ELM.",
                PiloteId = SeedIds.ResourceLefevre,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceProductionApplicative,
                TeamId = SeedIds.TeamProjetsA,
                StatusId = SeedIds.ProjectStatusActif,
                ProjectTypeId = SeedIds.ProjectTypeForfait,
                ClientId = SeedIds.ClientDirectionProduction,
                DateDebut = new DateOnly(2025, 2, 1),
                DateFinPrevueInitiale = new DateOnly(2025, 11, 30),
                BudgetInitial = 95000.00m,
                NiveauRisque = ProjectRiskLevel.Moyen,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectConsolidationReferentiels,
                Nom = "Consolidation Référentiels",
                Code = "PRJ-REF-2024",
                ApplicationId = SeedIds.AppServiceNow,
                DescriptionCourte = "Consolidation des référentiels applicatifs dans ServiceNow CMDB.",
                PiloteId = SeedIds.ResourceCosta,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceProjets,
                TeamId = SeedIds.TeamProjetsA,
                StatusId = SeedIds.ProjectStatusTermine,
                DateDebut = new DateOnly(2024, 1, 1),
                DateFinPrevueInitiale = new DateOnly(2024, 6, 30),
                DateFinReelle = new DateOnly(2024, 6, 28),
                BudgetInitial = 30000.00m,
                NiveauRisque = ProjectRiskLevel.Faible,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectArchiveLegacyVtom,
                Nom = "Archive Legacy VTOM",
                Code = "PRJ-VTOM-LEGACY-2023",
                ApplicationId = SeedIds.AppVtom,
                DescriptionCourte = "Décommissionnement de l'ancien ordonnanceur.",
                PiloteId = SeedIds.ResourceVerma,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceRunMco,
                StatusId = SeedIds.ProjectStatusArchive,
                DateDebut = new DateOnly(2023, 1, 1),
                DateFinPrevueInitiale = new DateOnly(2023, 12, 31),
                DateFinReelle = new DateOnly(2023, 12, 15),
                BudgetInitial = 15000.00m,
                NiveauRisque = ProjectRiskLevel.Faible,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            });
    }

    private static void SeedParticipants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectParticipant>().HasData(
            Participant(SeedIds.ParticipantLefevreOnVtomRefonte, SeedIds.ProjectRefonteVtom, SeedIds.ResourceLefevre, SeedIds.OpRoleChefDeProjet, 60.00m),
            Participant(SeedIds.ParticipantNguyenOnVtomRefonte, SeedIds.ProjectRefonteVtom, SeedIds.ResourceNguyen, SeedIds.OpRoleBuild, 40.00m),
            Participant(SeedIds.ParticipantManceronOnPortailRun, SeedIds.ProjectPortailRunServiceNow, SeedIds.ResourceManceron, SeedIds.OpRoleChefDeProjet, 50.00m),
            Participant(SeedIds.ParticipantNguyenOnPortailRun, SeedIds.ProjectPortailRunServiceNow, SeedIds.ResourceNguyen, SeedIds.OpRoleBuild, 40.00m),
            Participant(SeedIds.ParticipantFocquenoeyOnSupportN2, SeedIds.ProjectSupportServiceNowN2, SeedIds.ResourceFocquenoey, SeedIds.OpRoleRun, 30.00m),
            Participant(SeedIds.ParticipantPatelOnSupportN2, SeedIds.ProjectSupportServiceNowN2, SeedIds.ResourcePatel, null, 20.00m),
            Participant(SeedIds.ParticipantReauOnObservabilite, SeedIds.ProjectObservabiliteVtom, SeedIds.ResourceReau, SeedIds.OpRoleChefDeProjet, 35.00m),
            Participant(SeedIds.ParticipantDurandOnObservabilite, SeedIds.ProjectObservabiliteVtom, SeedIds.ResourceDurand, SeedIds.OpRoleBuild, 25.00m),
            Participant(SeedIds.ParticipantLefevreOnRefontePortailElm, SeedIds.ProjectRefontePortailElm, SeedIds.ResourceLefevre, SeedIds.OpRoleChefDeProjet, 45.00m),
            Participant(SeedIds.ParticipantCostaOnRefontePortailElm, SeedIds.ProjectRefontePortailElm, SeedIds.ResourceCosta, SeedIds.OpRoleBuild, 35.00m),
            Participant(SeedIds.ParticipantCostaOnConsolidation, SeedIds.ProjectConsolidationReferentiels, SeedIds.ResourceCosta, SeedIds.OpRoleChefDeProjet, 20.00m),
            Participant(SeedIds.ParticipantVermaOnConsolidation, SeedIds.ProjectConsolidationReferentiels, SeedIds.ResourceVerma, SeedIds.OpRoleBuild, 15.00m),
            Participant(SeedIds.ParticipantVermaOnArchiveLegacy, SeedIds.ProjectArchiveLegacyVtom, SeedIds.ResourceVerma, null, 10.00m));

        ProjectParticipant Participant(Guid id, Guid projectId, Guid resourceId, Guid? operationalRoleId, decimal capacitePrevue) => new()
        {
            Id = id,
            ProjectId = projectId,
            ResourceId = resourceId,
            OperationalRoleId = operationalRoleId,
            DateDebut = new DateOnly(2024, 1, 1),
            CapacitePrevue = capacitePrevue,
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedPlanningAndWeeklyPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectPlanVersion>().HasData(
            PlanVersion(SeedIds.PlanVersionVtomRefonteInitial, SeedIds.ProjectRefonteVtom, ProjectPlanVersionType.Initial, null),
            PlanVersion(SeedIds.PlanVersionPortailRunInitial, SeedIds.ProjectPortailRunServiceNow, ProjectPlanVersionType.Initial, null),
            PlanVersion(SeedIds.PlanVersionPortailRunAjuste, SeedIds.ProjectPortailRunServiceNow, ProjectPlanVersionType.Ajuste,
                "Charge revue à la hausse suite au périmètre étendu (démonstration)."),
            PlanVersion(SeedIds.PlanVersionObservabiliteInitial, SeedIds.ProjectObservabiliteVtom, ProjectPlanVersionType.Initial, null));

        // Deux semaines distinctes (2024-06-10 déjà utilisée par Lot4Seed pour Migration ELM,
        // 2024-06-17 la semaine suivante) pour démontrer la vue transverse "Planning projet"
        // (§18.2, GET /api/v1/project-planning) sur plusieurs semaines/projets/ressources.
        var week1 = new DateOnly(2024, 6, 10);
        var week2 = new DateOnly(2024, 6, 17);

        modelBuilder.Entity<ProjectWeeklyPlan>().HasData(
            WeeklyPlan(SeedIds.WeeklyPlanVtomRefonteInitialLefevreW1, SeedIds.PlanVersionVtomRefonteInitial, SeedIds.ResourceLefevre, week1, 24.00m),
            WeeklyPlan(SeedIds.WeeklyPlanVtomRefonteInitialNguyenW1, SeedIds.PlanVersionVtomRefonteInitial, SeedIds.ResourceNguyen, week1, 16.00m),
            WeeklyPlan(SeedIds.WeeklyPlanVtomRefonteInitialLefevreW2, SeedIds.PlanVersionVtomRefonteInitial, SeedIds.ResourceLefevre, week2, 24.00m),
            WeeklyPlan(SeedIds.WeeklyPlanPortailRunInitialManceronW1, SeedIds.PlanVersionPortailRunInitial, SeedIds.ResourceManceron, week1, 20.00m),
            WeeklyPlan(SeedIds.WeeklyPlanPortailRunAjusteManceronW1, SeedIds.PlanVersionPortailRunAjuste, SeedIds.ResourceManceron, week1, 28.00m),
            // Nguyen est déjà participant de Refonte VTOM et de Portail RUN ServiceNow (voir
            // SeedParticipants ci-dessus) : lui donner une ligne planifiée sur les deux projets
            // démontre la vue transverse "Planning projet" (§18.2) sur une même ressource multi-projets.
            WeeklyPlan(SeedIds.WeeklyPlanPortailRunInitialNguyenW1, SeedIds.PlanVersionPortailRunInitial, SeedIds.ResourceNguyen, week1, 12.00m),
            WeeklyPlan(SeedIds.WeeklyPlanObservabiliteInitialReauW1, SeedIds.PlanVersionObservabiliteInitial, SeedIds.ResourceReau, week1, 14.00m));

        ProjectPlanVersion PlanVersion(Guid id, Guid projectId, ProjectPlanVersionType type, string? motif) => new()
        {
            Id = id,
            ProjectId = projectId,
            Type = type,
            Statut = ProjectPlanVersionStatus.Active,
            Motif = motif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };

        ProjectWeeklyPlan WeeklyPlan(Guid id, Guid versionId, Guid resourceId, DateOnly weekStartDate, decimal heures) => new()
        {
            Id = id,
            ProjectPlanVersionId = versionId,
            ResourceId = resourceId,
            WeekStartDate = weekStartDate,
            ChargePlanifieeHeures = heures,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedMilestones(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Milestone>().HasData(
            Milestone(SeedIds.MilestoneVtomRefonteKickoff, "Kick-off Refonte VTOM", SeedIds.MilestoneTypeKickOff,
                SeedIds.ProjectRefonteVtom, SeedIds.AppVtom, SeedIds.ResourceLefevre,
                new DateOnly(2024, 2, 5), new DateOnly(2024, 2, 5), MilestoneStatus.Termine, MilestoneCriticality.Moyenne, null),
            Milestone(SeedIds.MilestoneVtomRefonteGoProd, "GO PROD Refonte VTOM", SeedIds.MilestoneTypeGoProd,
                SeedIds.ProjectRefonteVtom, SeedIds.AppVtom, SeedIds.ResourceLefevre,
                new DateOnly(2026, 10, 1), null, MilestoneStatus.AVenir, MilestoneCriticality.Elevee, null),

            Milestone(SeedIds.MilestonePortailRunKickoff, "Kick-off Portail RUN ServiceNow", SeedIds.MilestoneTypeKickOff,
                SeedIds.ProjectPortailRunServiceNow, SeedIds.AppServiceNow, SeedIds.ResourceManceron,
                new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10), MilestoneStatus.Termine, MilestoneCriticality.Moyenne, null),
            Milestone(SeedIds.MilestonePortailRunGoQual, "GO QUAL Portail RUN ServiceNow", SeedIds.MilestoneTypeGoQual,
                // Volontairement au statut EnCours avec une date prévue passée : démontre la
                // dérivation "en retard" (§24.2, MilestoneService.ToDto), jamais stockée.
                SeedIds.ProjectPortailRunServiceNow, SeedIds.AppServiceNow, SeedIds.ResourceManceron,
                new DateOnly(2025, 6, 1), null, MilestoneStatus.EnCours, MilestoneCriticality.Critique, null),

            Milestone(SeedIds.MilestoneSupportN2Kickoff, "Kick-off Support ServiceNow N2", SeedIds.MilestoneTypeKickOff,
                SeedIds.ProjectSupportServiceNowN2, SeedIds.AppServiceNow, SeedIds.ResourceFocquenoey,
                new DateOnly(2025, 3, 5), null, MilestoneStatus.AVenir, MilestoneCriticality.Faible, null),

            Milestone(SeedIds.MilestoneObservabiliteArchitecture, "Architecture Observabilité VTOM", SeedIds.MilestoneTypeArchitecture,
                SeedIds.ProjectObservabiliteVtom, SeedIds.AppVtom, SeedIds.ResourceReau,
                new DateOnly(2024, 7, 1), new DateOnly(2024, 7, 3), MilestoneStatus.Termine, MilestoneCriticality.Moyenne, null),
            // Dépend du jalon Architecture ci-dessus (§24.2 "dépendance éventuelle") : démontre
            // l'affichage d'une dépendance de jalon sans détection de cycle (Lot 4, non demandée).
            Milestone(SeedIds.MilestoneObservabiliteGoProd, "GO PROD Observabilité VTOM", SeedIds.MilestoneTypeGoProd,
                SeedIds.ProjectObservabiliteVtom, SeedIds.AppVtom, SeedIds.ResourceReau,
                new DateOnly(2025, 5, 31), null, MilestoneStatus.EnCours, MilestoneCriticality.Elevee, SeedIds.MilestoneObservabiliteArchitecture),

            Milestone(SeedIds.MilestoneRefontePortailElmKickoff, "Kick-off Refonte Portail ELM", SeedIds.MilestoneTypeKickOff,
                SeedIds.ProjectRefontePortailElm, SeedIds.AppIbmElm, SeedIds.ResourceLefevre,
                new DateOnly(2025, 2, 10), new DateOnly(2025, 2, 10), MilestoneStatus.Termine, MilestoneCriticality.Moyenne, null),
            Milestone(SeedIds.MilestoneRefontePortailElmVabe, "VABE Refonte Portail ELM", SeedIds.MilestoneTypeVabe,
                SeedIds.ProjectRefontePortailElm, SeedIds.AppIbmElm, SeedIds.ResourceLefevre,
                new DateOnly(2025, 10, 15), null, MilestoneStatus.AVenir, MilestoneCriticality.Elevee, null),

            Milestone(SeedIds.MilestoneConsolidationKickoff, "Kick-off Consolidation Référentiels", SeedIds.MilestoneTypeKickOff,
                SeedIds.ProjectConsolidationReferentiels, SeedIds.AppServiceNow, SeedIds.ResourceCosta,
                new DateOnly(2024, 1, 8), new DateOnly(2024, 1, 8), MilestoneStatus.Termine, MilestoneCriticality.Faible, null),
            Milestone(SeedIds.MilestoneConsolidationGoProd, "GO PROD Consolidation Référentiels", SeedIds.MilestoneTypeGoProd,
                SeedIds.ProjectConsolidationReferentiels, SeedIds.AppServiceNow, SeedIds.ResourceCosta,
                new DateOnly(2024, 6, 25), new DateOnly(2024, 6, 25), MilestoneStatus.Termine, MilestoneCriticality.Moyenne, null),

            Milestone(SeedIds.MilestoneArchiveLegacyCab, "CAB Archive Legacy VTOM", SeedIds.MilestoneTypeCab,
                SeedIds.ProjectArchiveLegacyVtom, SeedIds.AppVtom, SeedIds.ResourceVerma,
                new DateOnly(2023, 12, 10), new DateOnly(2023, 12, 10), MilestoneStatus.Termine, MilestoneCriticality.Elevee, null));

        Milestone Milestone(
            Guid id, string nom, Guid milestoneTypeId, Guid projectId, Guid applicationId, Guid responsableId,
            DateOnly datePrevue, DateOnly? dateReelle, MilestoneStatus statut, MilestoneCriticality criticite, Guid? dependsOnMilestoneId) => new()
        {
            Id = id,
            Nom = nom,
            MilestoneTypeId = milestoneTypeId,
            ProjectId = projectId,
            ApplicationId = applicationId,
            ResponsableId = responsableId,
            DatePrevue = datePrevue,
            DateReelle = dateReelle,
            Statut = statut,
            Criticite = criticite,
            DependsOnMilestoneId = dependsOnMilestoneId,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedBudgets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>().HasData(
            Budget(SeedIds.BudgetVtomRefonte, "Budget Refonte VTOM", SeedIds.ProjectRefonteVtom, 80000.00m, new DateOnly(2024, 2, 1)),
            Budget(SeedIds.BudgetPortailRunServiceNow, "Budget Portail RUN ServiceNow", SeedIds.ProjectPortailRunServiceNow, 60000.00m, new DateOnly(2025, 1, 1)),
            Budget(SeedIds.BudgetRefontePortailElm, "Budget Refonte Portail ELM", SeedIds.ProjectRefontePortailElm, 95000.00m, new DateOnly(2025, 2, 1)));

        Budget Budget(Guid id, string name, Guid projectId, decimal amount, DateOnly startDate) => new()
        {
            Id = id,
            Name = name,
            ProjectId = projectId,
            InitialAmount = amount,
            AdjustedAmount = amount,
            Status = BudgetStatus.Actif,
            AlertThreshold = 90m,
            StartDate = startDate,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }
}
