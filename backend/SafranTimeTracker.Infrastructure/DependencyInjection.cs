using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafranTimeTracker.Infrastructure.Persistence;

namespace SafranTimeTracker.Infrastructure;

/// <summary>
/// Point d'entrée unique d'enregistrement des services de la couche Infrastructure dans le conteneur DI.
/// Le mapping physique (snake_case) est appliqué uniformément aux trois providers pour éviter
/// toute divergence de modèle logique entre eux (voir docs/DATABASE.md §2).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration["Database:Provider"] ?? nameof(DatabaseProvider.Sqlite);
        if (!Enum.TryParse<DatabaseProvider>(providerName, ignoreCase: true, out var provider))
        {
            throw new InvalidOperationException(
                $"Provider de base de données inconnu : '{providerName}'. Valeurs attendues : {string.Join(", ", Enum.GetNames<DatabaseProvider>())}.");
        }

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("La chaîne de connexion 'ConnectionStrings:Default' est requise.");

        services.AddDbContext<AppDbContext>(options =>
        {
            // Chaque provider pointe vers son propre assembly de migrations (projets sous database/) :
            // cela évite qu'EF Core ne tente d'appliquer, au runtime, des migrations écrites pour un
            // autre moteur (voir docs/DATABASE.md §1 et les projets SafranTimeTracker.Migrations.*).
            switch (provider)
            {
                case DatabaseProvider.PostgreSql:
                    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.PostgreSql"));
                    break;
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.SqlServer"));
                    break;
                case DatabaseProvider.Sqlite:
                default:
                    options.UseSqlite(connectionString, b => b.MigrationsAssembly("SafranTimeTracker.Migrations.Sqlite"));
                    break;
            }

            options.UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
