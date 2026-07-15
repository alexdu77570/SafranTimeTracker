using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Migrations.Sqlite;

/// <summary>
/// Utilisée uniquement par l'outillage `dotnet ef` pour générer/appliquer les migrations SQLite
/// (développement local et tests). Jamais utilisée à l'exécution normale de l'API, qui passe par
/// SafranTimeTracker.Infrastructure.DependencyInjection.AddInfrastructure et la configuration réelle.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
            ?? "Data Source=safrantimetracker.designtime.db";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.Sqlite"));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
