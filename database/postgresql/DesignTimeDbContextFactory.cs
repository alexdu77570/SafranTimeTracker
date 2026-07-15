using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Migrations.PostgreSql;

/// <summary>
/// Utilisée uniquement par l'outillage `dotnet ef` pour générer/appliquer les migrations PostgreSQL
/// (provider principal). Jamais utilisée à l'exécution normale de l'API, qui passe par
/// SafranTimeTracker.Infrastructure.DependencyInjection.AddInfrastructure et la configuration réelle.
/// La chaîne ci-dessous est une valeur de développement local par défaut (pas un secret réel) :
/// elle ne sert qu'à la génération de migrations hors ligne, sans connexion effective requise.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
            ?? "Host=localhost;Database=safrantimetracker_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.PostgreSql"));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
