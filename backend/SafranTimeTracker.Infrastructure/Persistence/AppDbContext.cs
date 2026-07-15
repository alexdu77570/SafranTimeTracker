using Microsoft.EntityFrameworkCore;

namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>
/// Contexte EF Core unique de l'application. Volontairement dépourvu de DbSet en Lot 0 :
/// aucune entité métier du cahier des charges n'est créée avant le Lot 1 (voir docs/ROADMAP.md).
/// Les configurations d'entités (IEntityTypeConfiguration&lt;T&gt;) seront ajoutées dans ce même
/// assembly et appliquées automatiquement via ApplyConfigurationsFromAssembly.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
