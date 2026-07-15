using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Reporting;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 5 : la permission TIME_ENTRY_CORRECTION (§13.4), une
/// rallonge sur la commande de démonstration du Lot 1 (§13.3 — OrderDemo.BudgetFinancierAjuste/
/// BudgetJoursAjuste/DateFinAjustee dans Lot1Seed reflètent déjà l'état post-rallonge), un budget
/// et son historique d'ajustement liés à "Migration ELM" (§14), et le référentiel des KPI de
/// tableau de bord (§25.1/§25.2). Idempotent (HasData), dates/montants strictement déterministes.
/// </summary>
internal static class Lot5Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedPermission(modelBuilder);
        SeedOrderExtension(modelBuilder);
        SeedBudget(modelBuilder);
        SeedDashboardKpis(modelBuilder);
    }

    private static void SeedPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(new Permission
        {
            Id = SeedIds.PermissionTimeEntryCorrection,
            Code = "TIME_ENTRY_CORRECTION",
            Libelle = "Correction de saisie sur commande clôturée",
            Description = "Autorise une saisie de temps sur une commande clôturée, à titre de correction (cahier des charges §13.4)."
        });

        modelBuilder.Entity<UserPermission>().HasData(
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionTimeEntryCorrection, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor });
    }

    /// <summary>Rallonge historique : porte les mêmes montants que ceux déjà reflétés sur
    /// OrderDemo (Lot1Seed) — la donnée n'est pas recalculée à partir de cette ligne, elle
    /// documente simplement comment on y est arrivé (§13.3 "visible dans l'historique").</summary>
    private static void SeedOrderExtension(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderExtension>().HasData(new OrderExtension
        {
            Id = SeedIds.OrderExtensionDemo,
            OrderId = SeedIds.OrderDemo,
            ExtensionDate = new DateOnly(2026, 6, 1),
            AmountAdded = 20000.00m,
            DaysAdded = 30m,
            PreviousEndDate = new DateOnly(2026, 12, 31),
            NewEndDate = new DateOnly(2027, 3, 31),
            Reason = "Extension de périmètre validée par le comité de pilotage (démonstration).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedBudget(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>().HasData(new Budget
        {
            Id = SeedIds.BudgetMigrationElm,
            Name = "Budget Migration ELM",
            ProjectId = SeedIds.ProjectMigrationElm,
            InitialAmount = 150000.00m,
            // Reflète l'ajustement déjà historisé par BudgetVersionMigrationElm ci-dessous.
            AdjustedAmount = 180000.00m,
            Status = BudgetStatus.Actif,
            AlertThreshold = 90m,
            StartDate = new DateOnly(2024, 1, 1),
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });

        modelBuilder.Entity<BudgetVersion>().HasData(new BudgetVersion
        {
            Id = SeedIds.BudgetVersionMigrationElm,
            BudgetId = SeedIds.BudgetMigrationElm,
            OldValue = 150000.00m,
            NewValue = 180000.00m,
            Reason = "Dérive planning nécessitant un budget ajusté (démonstration, cf. Project.DateFinAjustee).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedDashboardKpis(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DashboardKpi>().HasData(
            Kpi(SeedIds.DashboardKpiTempsSaisis, "TEMPS_SAISIS", "Temps saisis sur la période", DashboardKpiCategory.Operationnel, 1),
            Kpi(SeedIds.DashboardKpiCapaciteReelle, "CAPACITE_REELLE", "Capacité réelle", DashboardKpiCategory.Operationnel, 2),
            Kpi(SeedIds.DashboardKpiTauxDisponibilite, "TAUX_DISPONIBILITE", "Taux de disponibilité", DashboardKpiCategory.Operationnel, 3),
            Kpi(SeedIds.DashboardKpiChargeRunHorsRun, "CHARGE_RUN_HORS_RUN", "Charge RUN / hors RUN", DashboardKpiCategory.Operationnel, 4),
            Kpi(SeedIds.DashboardKpiProjetsActifs, "PROJETS_ACTIFS", "Projets actifs", DashboardKpiCategory.Operationnel, 5),
            Kpi(SeedIds.DashboardKpiJalonsEnRetard, "JALONS_EN_RETARD", "Jalons en retard", DashboardKpiCategory.Operationnel, 6),
            Kpi(SeedIds.DashboardKpiBudgetRestant, "BUDGET_RESTANT", "Budget restant", DashboardKpiCategory.Financier, 7),
            Kpi(SeedIds.DashboardKpiDifferentielGlobal, "DIFFERENTIEL_GLOBAL", "Différentiel global", DashboardKpiCategory.Financier, 8));

        DashboardKpi Kpi(Guid id, string code, string libelle, DashboardKpiCategory category, int ordre) => new()
        {
            Id = id,
            Code = code,
            Libelle = libelle,
            Category = category,
            Ordre = ordre,
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }
}
