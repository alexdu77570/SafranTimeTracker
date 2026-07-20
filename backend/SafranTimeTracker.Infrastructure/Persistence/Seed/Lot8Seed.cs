using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Clients;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Currencies;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Projects;
using SafranTimeTracker.Domain.Technologies;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 8 (docs/BACKLOG_METIER.md §5-9) : 3 technologies (dont
/// deux rattachées à IBM ELM et une à une ressource), 2 clients, 3 types de projet, 2 centres de
/// coûts rattachés à l'organisation du Lot 1, 2 devises. Idempotent (HasData), dates
/// strictement déterministes. ProjectMigrationElm (Lot 4) est complétée avec un type de projet et
/// un client — voir Lot4Seed.cs.
/// </summary>
internal static class Lot8Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedTechnologies(modelBuilder);
        SeedClients(modelBuilder);
        SeedProjectTypes(modelBuilder);
        SeedCostCenters(modelBuilder);
        SeedCurrencies(modelBuilder);
    }

    private static void SeedTechnologies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Technology>().HasData(
            new Technology { Id = SeedIds.TechnologyDotNet, Code = "DOTNET", Libelle = ".NET", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Technology { Id = SeedIds.TechnologyReact, Code = "REACT", Libelle = "React", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Technology { Id = SeedIds.TechnologyPostgresql, Code = "POSTGRESQL", Libelle = "PostgreSQL", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });

        modelBuilder.Entity<ApplicationTechnology>().HasData(
            new ApplicationTechnology { ApplicationId = SeedIds.AppIbmElm, TechnologyId = SeedIds.TechnologyDotNet },
            new ApplicationTechnology { ApplicationId = SeedIds.AppIbmElm, TechnologyId = SeedIds.TechnologyPostgresql });

        modelBuilder.Entity<ResourceTechnology>().HasData(
            new ResourceTechnology { ResourceId = SeedIds.ResourceGeorges, TechnologyId = SeedIds.TechnologyDotNet },
            new ResourceTechnology { ResourceId = SeedIds.ResourceGeorges, TechnologyId = SeedIds.TechnologyReact });
    }

    private static void SeedClients(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().HasData(
            new Client { Id = SeedIds.ClientDirectionProduction, Code = "DIR-PROD", Nom = "Direction Production Applicative", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Client { Id = SeedIds.ClientDirectionSupport, Code = "DIR-SUPPORT", Nom = "Direction Support et Exploitation", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }

    private static void SeedProjectTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectType>().HasData(
            new ProjectType { Id = SeedIds.ProjectTypeForfait, Code = "FORFAIT", Libelle = "Forfait", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new ProjectType { Id = SeedIds.ProjectTypeRegie, Code = "REGIE", Libelle = "Régie", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new ProjectType { Id = SeedIds.ProjectTypeInterne, Code = "INTERNE", Libelle = "Interne", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }

    private static void SeedCostCenters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CostCenter>().HasData(
            new CostCenter { Id = SeedIds.CostCenterDsi, Code = "CC-DSI", Libelle = "Centre de coûts DSI", DepartmentId = SeedIds.DepartmentDsi, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new CostCenter { Id = SeedIds.CostCenterProductionApplicative, Code = "CC-PRODAPP", Libelle = "Centre de coûts Production Applicative", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }

    private static void SeedCurrencies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Id = SeedIds.CurrencyEur, CodeIso = "EUR", Libelle = "Euro", Symbole = "€", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Currency { Id = SeedIds.CurrencyUsd, CodeIso = "USD", Libelle = "Dollar américain", Symbole = "$", Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }
}
