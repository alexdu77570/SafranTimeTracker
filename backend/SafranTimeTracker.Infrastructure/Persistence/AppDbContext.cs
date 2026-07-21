using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Absences;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Auditing;
using SafranTimeTracker.Domain.Budgets;
using SafranTimeTracker.Domain.Clients;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Currencies;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Milestones;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Reporting;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Technologies;
using SafranTimeTracker.Domain.Time;
using SafranTimeTracker.Domain.Users;
using SafranTimeTracker.Infrastructure.Persistence.Seed;
using HolidayCalendarEntity = SafranTimeTracker.Domain.Settings.HolidayCalendar;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core unique de l'application. Porte les référentiels du Lot 1, le modèle financier
/// du Lot 2, le temps/capacité du Lot 3, les projets du Lot 4, les budgets/reporting du Lot 5 et
/// les imports/audit du Lot 6 (docs/ROADMAP.md). Les configurations d'entités
/// (IEntityTypeConfiguration&lt;T&gt;) sont appliquées automatiquement via
/// ApplyConfigurationsFromAssembly.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Organisation
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Team> Teams => Set<Team>();

    // Utilisateurs et sécurité
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    // Ressources
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<OperationalRole> OperationalRoles => Set<OperationalRole>();
    public DbSet<ResourceOperationalRole> ResourceOperationalRoles => Set<ResourceOperationalRole>();

    // Applications
    public DbSet<ApplicationReference> ApplicationReferences => Set<ApplicationReference>();

    // Sociétés
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyType> CompanyTypes => Set<CompanyType>();

    // Commandes
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<OrderAuthorizedResource> OrderAuthorizedResources => Set<OrderAuthorizedResource>();

    // Paramètres
    public DbSet<SettingsEntity> Settings => Set<SettingsEntity>();

    // Modèle financier (Lot 2)
    public DbSet<ResourceTjmHistory> ResourceTjmHistories => Set<ResourceTjmHistory>();
    public DbSet<CompanyContractHistory> CompanyContractHistories => Set<CompanyContractHistory>();
    public DbSet<ResourceCompanyAssignment> ResourceCompanyAssignments => Set<ResourceCompanyAssignment>();

    // Temps et capacité (Lot 3)
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<TimeEntryFinancialSnapshot> TimeEntryFinancialSnapshots => Set<TimeEntryFinancialSnapshot>();
    public DbSet<Absence> Absences => Set<Absence>();
    public DbSet<ResourceCapacityPeriod> ResourceCapacityPeriods => Set<ResourceCapacityPeriod>();
    public DbSet<HolidayCalendarEntity> HolidayCalendar => Set<HolidayCalendarEntity>();

    // Projets (Lot 4)
    public DbSet<ProjectStatus> ProjectStatuses => Set<ProjectStatus>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectParticipant> ProjectParticipants => Set<ProjectParticipant>();
    public DbSet<ProjectPlanVersion> ProjectPlanVersions => Set<ProjectPlanVersion>();
    public DbSet<ProjectWeeklyPlan> ProjectWeeklyPlans => Set<ProjectWeeklyPlan>();
    public DbSet<MilestoneType> MilestoneTypes => Set<MilestoneType>();
    public DbSet<Milestone> Milestones => Set<Milestone>();

    // Budgets et reporting (Lot 5)
    public DbSet<OrderExtension> OrderExtensions => Set<OrderExtension>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetVersion> BudgetVersions => Set<BudgetVersion>();
    public DbSet<DashboardKpi> DashboardKpis => Set<DashboardKpi>();
    public DbSet<ExportLog> ExportLogs => Set<ExportLog>();

    // Imports et audit (Lot 6)
    public DbSet<OrderReceipt> OrderReceipts => Set<OrderReceipt>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportDiff> ImportDiffs => Set<ImportDiff>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Référentiels et administration (Lot 8)
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<ApplicationTechnology> ApplicationTechnologies => Set<ApplicationTechnology>();
    public DbSet<ResourceTechnology> ResourceTechnologies => Set<ResourceTechnology>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ProjectType> ProjectTypes => Set<ProjectType>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        Lot1Seed.Apply(modelBuilder);
        Lot2Seed.Apply(modelBuilder);
        Lot3Seed.Apply(modelBuilder);
        Lot4Seed.Apply(modelBuilder);
        Lot5Seed.Apply(modelBuilder);
        Lot6Seed.Apply(modelBuilder);
        Lot8Seed.Apply(modelBuilder);
        Lot10Seed.Apply(modelBuilder);
    }
}
