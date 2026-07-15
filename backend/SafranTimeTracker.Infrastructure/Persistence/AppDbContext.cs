using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Absences;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Time;
using SafranTimeTracker.Domain.Users;
using SafranTimeTracker.Infrastructure.Persistence.Seed;
using HolidayCalendarEntity = SafranTimeTracker.Domain.Settings.HolidayCalendar;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core unique de l'application. Porte les référentiels du Lot 1, le modèle financier
/// du Lot 2 et le temps/capacité du Lot 3 (docs/ROADMAP.md) : aucune entité de projet ou de budget
/// n'est encore présente (Lots 4 à 5). Les configurations d'entités
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        Lot1Seed.Apply(modelBuilder);
        Lot2Seed.Apply(modelBuilder);
        Lot3Seed.Apply(modelBuilder);
    }
}
