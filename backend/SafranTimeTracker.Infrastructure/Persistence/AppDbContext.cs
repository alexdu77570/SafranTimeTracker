using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Users;
using SafranTimeTracker.Infrastructure.Persistence.Seed;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core unique de l'application. Porte les référentiels du Lot 1 (docs/ROADMAP.md) :
/// aucune entité financière historisée (TJM, contrats), de temps, de projet ou de budget n'est
/// encore présente (Lots 2 à 5). Les configurations d'entités (IEntityTypeConfiguration&lt;T&gt;)
/// sont appliquées automatiquement via ApplyConfigurationsFromAssembly.
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        Lot1Seed.Apply(modelBuilder);
    }
}
