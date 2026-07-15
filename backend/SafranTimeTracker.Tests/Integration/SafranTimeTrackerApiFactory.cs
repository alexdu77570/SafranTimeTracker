using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Tests.Integration;

/// <summary>
/// Hôte de test partagé : base SQLite dédiée par instance de fixture, migrée (et donc semée,
/// voir Lot1Seed) une seule fois avant les tests d'une classe. Isolée de la base de
/// développement (App_Data) et supprimée en fin de tests.
/// </summary>
public class SafranTimeTrackerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"safrantimetracker-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // ConfigureAppConfiguration + AddInMemoryCollection n'obtient pas la priorité sur
        // appsettings.{Environment}.json avec l'hébergement minimal (Program.cs à instructions de
        // haut niveau) : UseSetting écrit directement dans les paramètres d'hôte, avec une
        // précédence garantie sur les fichiers de configuration.
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch (IOException)
        {
            // Le fichier peut rester brièvement verrouillé (pool de connexions SQLite) : sans
            // conséquence, il s'agit d'un fichier temporaire que l'OS nettoie de toute façon.
        }
    }
}
