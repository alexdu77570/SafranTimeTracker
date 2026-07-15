using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Migrations.SqlServer;

/// <summary>
/// Utilisée uniquement par l'outillage `dotnet ef` pour générer/appliquer les migrations SQL Server
/// (provider alternatif). Jamais utilisée à l'exécution normale de l'API, qui passe par
/// SafranTimeTracker.Infrastructure.DependencyInjection.AddInfrastructure et la configuration réelle.
/// La chaîne ci-dessous est une valeur de développement local par défaut (pas un secret réel).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
            ?? "Server=localhost;Database=SafranTimeTrackerDev;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.SqlServer"));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
