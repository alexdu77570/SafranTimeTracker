using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 4 : les 4 statuts de projet et les 11 types de jalon
/// (§16.2, §24.1), deux projets dont un pleinement instrumenté (participants, version Initiale +
/// version Ajustée active avec grille hebdomadaire, jalons couvrant Terminé/en retard/à venir).
/// TimeEntryLegrandProjet (Lot 3) est rattachée à ProjectMigrationElm pour démontrer l'agrégation
/// charge/coût consommés à partir d'une saisie réelle. Idempotent (HasData), dates/montants
/// strictement déterministes.
/// </summary>
internal static class Lot4Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedProjectStatuses(modelBuilder);
        SeedMilestoneTypes(modelBuilder);
        SeedProjects(modelBuilder);
        SeedParticipants(modelBuilder);
        SeedPlanningAndWeeklyPlans(modelBuilder);
        SeedMilestones(modelBuilder);
    }

    private static void SeedProjectStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectStatus>().HasData(
            new ProjectStatus { Id = SeedIds.ProjectStatusActif, Code = "ACTIF", Libelle = "Actif", Ordre = 1 },
            new ProjectStatus { Id = SeedIds.ProjectStatusSuspendu, Code = "SUSPENDU", Libelle = "Suspendu", Ordre = 2 },
            new ProjectStatus { Id = SeedIds.ProjectStatusTermine, Code = "TERMINE", Libelle = "Terminé", Ordre = 3 },
            new ProjectStatus { Id = SeedIds.ProjectStatusArchive, Code = "ARCHIVE", Libelle = "Archivé", Ordre = 4 });
    }

    private static void SeedMilestoneTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MilestoneType>().HasData(
            MilestoneType(SeedIds.MilestoneTypeKickOff, "KICKOFF", "Kick-off"),
            MilestoneType(SeedIds.MilestoneTypeArchitecture, "ARCHITECTURE", "Architecture"),
            MilestoneType(SeedIds.MilestoneTypeVabe, "VABE", "VABE"),
            MilestoneType(SeedIds.MilestoneTypeVsr, "VSR", "VSR"),
            MilestoneType(SeedIds.MilestoneTypeGoDev, "GO_DEV", "GO DEV"),
            MilestoneType(SeedIds.MilestoneTypeGoQual, "GO_QUAL", "GO QUAL"),
            MilestoneType(SeedIds.MilestoneTypeGoVal, "GO_VAL", "GO VAL"),
            MilestoneType(SeedIds.MilestoneTypeGoPprod, "GO_PPROD", "GO PPROD"),
            MilestoneType(SeedIds.MilestoneTypeGoProd, "GO_PROD", "GO PROD"),
            MilestoneType(SeedIds.MilestoneTypeCab, "CAB", "CAB"),
            MilestoneType(SeedIds.MilestoneTypeHypercare, "HYPERCARE", "Hypercare"));

        MilestoneType MilestoneType(Guid id, string code, string libelle) => new()
        {
            Id = id,
            Code = code,
            Libelle = libelle,
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = SeedIds.ProjectMigrationElm,
                Nom = "Migration ELM",
                Code = "PRJ-ELM-2026",
                ApplicationId = SeedIds.AppIbmElm,
                DescriptionCourte = "Migration de la plateforme IBM ELM.",
                PiloteId = SeedIds.ResourceGeorges,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceProjets,
                TeamId = SeedIds.TeamProjetsA,
                StatusId = SeedIds.ProjectStatusActif,
                DateDebut = new DateOnly(2024, 1, 1),
                DateFinPrevueInitiale = new DateOnly(2024, 12, 31),
                DateFinAjustee = new DateOnly(2025, 3, 31), // dérive planning -> risque planning (§29.5)
                BudgetInitial = 150000.00m,
                NiveauRisque = ProjectRiskLevel.Moyen,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Project
            {
                Id = SeedIds.ProjectRefonteVtom,
                Nom = "Refonte VTOM",
                Code = "PRJ-VTOM-2026",
                ApplicationId = SeedIds.AppVtom,
                DescriptionCourte = "Refonte de l'ordonnanceur VTOM.",
                PiloteId = SeedIds.ResourceLefevre,
                DepartmentId = SeedIds.DepartmentDsi,
                ServiceId = SeedIds.ServiceRunMco,
                TeamId = SeedIds.TeamRunA,
                StatusId = SeedIds.ProjectStatusActif,
                DateDebut = new DateOnly(2024, 2, 1),
                DateFinPrevueInitiale = new DateOnly(2024, 11, 30),
                BudgetInitial = 80000.00m,
                NiveauRisque = ProjectRiskLevel.Faible,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            });
    }

    private static void SeedParticipants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectParticipant>().HasData(
            new ProjectParticipant
            {
                Id = SeedIds.ParticipantGeorgesOnElm,
                ProjectId = SeedIds.ProjectMigrationElm,
                ResourceId = SeedIds.ResourceGeorges,
                OperationalRoleId = SeedIds.OpRoleChefDeProjet,
                DateDebut = new DateOnly(2024, 1, 1),
                CapacitePrevue = 100.00m,
                Statut = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new ProjectParticipant
            {
                Id = SeedIds.ParticipantLegrandOnElm,
                ProjectId = SeedIds.ProjectMigrationElm,
                ResourceId = SeedIds.ResourceLegrand,
                DateDebut = new DateOnly(2024, 1, 1),
                CapacitePrevue = 50.00m,
                Statut = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            });
    }

    private static void SeedPlanningAndWeeklyPlans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectPlanVersion>().HasData(
            new ProjectPlanVersion
            {
                Id = SeedIds.PlanVersionElmInitial,
                ProjectId = SeedIds.ProjectMigrationElm,
                Type = ProjectPlanVersionType.Initial,
                Statut = ProjectPlanVersionStatus.Active,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new ProjectPlanVersion
            {
                Id = SeedIds.PlanVersionElmAjuste,
                ProjectId = SeedIds.ProjectMigrationElm,
                Type = ProjectPlanVersionType.Ajuste,
                Statut = ProjectPlanVersionStatus.Active,
                Motif = "Report de la fin de projet, charge revue à la hausse (démonstration).",
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            });

        // Semaine du 2024-06-10 (lundi), qui contient la saisie réelle TimeEntryLegrandProjet.
        var weekStart = new DateOnly(2024, 6, 10);

        modelBuilder.Entity<ProjectWeeklyPlan>().HasData(
            WeeklyPlan(SeedIds.WeeklyPlanElmInitialGeorgesW1, SeedIds.PlanVersionElmInitial, SeedIds.ResourceGeorges, weekStart, 20.00m),
            WeeklyPlan(SeedIds.WeeklyPlanElmInitialLegrandW1, SeedIds.PlanVersionElmInitial, SeedIds.ResourceLegrand, weekStart, 10.00m),
            WeeklyPlan(SeedIds.WeeklyPlanElmAjusteGeorgesW1, SeedIds.PlanVersionElmAjuste, SeedIds.ResourceGeorges, weekStart, 24.00m),
            WeeklyPlan(SeedIds.WeeklyPlanElmAjusteLegrandW1, SeedIds.PlanVersionElmAjuste, SeedIds.ResourceLegrand, weekStart, 8.00m));

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
            new Milestone
            {
                Id = SeedIds.MilestoneElmKickoff,
                Nom = "Kick-off Migration ELM",
                MilestoneTypeId = SeedIds.MilestoneTypeKickOff,
                ProjectId = SeedIds.ProjectMigrationElm,
                ApplicationId = SeedIds.AppIbmElm,
                ResponsableId = SeedIds.ResourceGeorges,
                DatePrevue = new DateOnly(2024, 1, 15),
                DateReelle = new DateOnly(2024, 1, 15),
                Statut = MilestoneStatus.Termine,
                Criticite = MilestoneCriticality.Moyenne,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Milestone
            {
                // Volontairement au statut EnCours avec une date prévue passée : démontre la
                // dérivation "en retard" (§24.2, MilestoneService.ToDto), jamais stockée.
                Id = SeedIds.MilestoneElmGoProd,
                Nom = "GO PROD Migration ELM",
                MilestoneTypeId = SeedIds.MilestoneTypeGoProd,
                ProjectId = SeedIds.ProjectMigrationElm,
                ApplicationId = SeedIds.AppIbmElm,
                ResponsableId = SeedIds.ResourceGeorges,
                DatePrevue = new DateOnly(2025, 6, 1),
                Statut = MilestoneStatus.EnCours,
                Criticite = MilestoneCriticality.Critique,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            },
            new Milestone
            {
                Id = SeedIds.MilestoneElmCab,
                Nom = "CAB Migration ELM",
                MilestoneTypeId = SeedIds.MilestoneTypeCab,
                ProjectId = SeedIds.ProjectMigrationElm,
                ApplicationId = SeedIds.AppIbmElm,
                ResponsableId = SeedIds.ResourceGeorges,
                DatePrevue = new DateOnly(2027, 1, 1),
                Statut = MilestoneStatus.AVenir,
                Criticite = MilestoneCriticality.Elevee,
                CreatedAt = SeedTimestamp,
                CreatedBy = SeedAuthor
            });
    }
}
