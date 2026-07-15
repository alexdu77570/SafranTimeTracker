using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 2 : historiques TJM, une société externe avec son
/// historique de contrat, et des rattachements ressource/société — couvrant volontairement les
/// cas décrits par docs/DATABASE.md §7 (historisation d'un TJM dans le temps, société interne
/// "non applicable", société externe valorisable, valorisation incomplète). Idempotent (HasData),
/// dates/horodatage strictement déterministes.
/// </summary>
internal static class Lot2Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedExternalCompany(modelBuilder);
        SeedTjmHistory(modelBuilder);
        SeedContractHistory(modelBuilder);
        SeedResourceCompanyAssignments(modelBuilder);
    }

    private static void SeedExternalCompany(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = SeedIds.CompanyExterneConseil,
            Nom = "Externe Conseil",
            Code = "EXTCONSEIL",
            CompanyTypeId = SeedIds.CompanyTypeExterne,
            Statut = ReferentialStatus.Actif,
            ContactPrincipal = "Direction Commerciale",
            EmailContact = "contact@externeconseil.local",
            Commentaire = "Société externe de démonstration (données de démonstration, Lot 2).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedTjmHistory(ModelBuilder modelBuilder)
    {
        // BERNARD : période ouverte -> valorisation complète.
        // LEGRAND : période ouverte -> valorisation complète (société externe, cf. contrats).
        // GEORGES : deux périodes successives non chevauchantes -> démontre l'historisation.
        // MISHRA : volontairement sans historique TJM -> valorisation incomplète (§11.4).
        // ConcurrencyStamp fixé explicitement (Guid.Empty) : le défaut Guid.NewGuid() de l'entité
        // (destiné aux créations applicatives) romprait l'idempotence du seed d'une régénération
        // de migration à l'autre (CLAUDE.md §11).
        modelBuilder.Entity<ResourceTjmHistory>().HasData(
            new ResourceTjmHistory
            {
                Id = SeedIds.TjmBernard, ResourceId = SeedIds.ResourceBernard,
                StartDate = new DateOnly(2024, 1, 1), EndDate = null, DailyRate = 650.00m,
                Reason = "TJM initial", Status = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor, ConcurrencyStamp = Guid.Empty
            },
            new ResourceTjmHistory
            {
                Id = SeedIds.TjmLegrand, ResourceId = SeedIds.ResourceLegrand,
                StartDate = new DateOnly(2024, 1, 1), EndDate = null, DailyRate = 700.00m,
                Reason = "TJM initial", Status = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor, ConcurrencyStamp = Guid.Empty
            },
            new ResourceTjmHistory
            {
                Id = SeedIds.TjmGeorges1, ResourceId = SeedIds.ResourceGeorges,
                StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31), DailyRate = 600.00m,
                Reason = "TJM initial", Status = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor, ConcurrencyStamp = Guid.Empty
            },
            new ResourceTjmHistory
            {
                Id = SeedIds.TjmGeorges2, ResourceId = SeedIds.ResourceGeorges,
                StartDate = new DateOnly(2025, 1, 1), EndDate = null, DailyRate = 620.00m,
                Reason = "Revalorisation annuelle", Status = ReferentialStatus.Actif,
                CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor, ConcurrencyStamp = Guid.Empty
            });
    }

    private static void SeedContractHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompanyContractHistory>().HasData(new CompanyContractHistory
        {
            Id = SeedIds.ContractExterneConseil, CompanyId = SeedIds.CompanyExterneConseil,
            ContractNumber = "CTR-2024-001", StartDate = new DateOnly(2024, 1, 1), EndDate = null,
            ContractDailyRate = 750.00m, Currency = "EUR", Status = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor, ConcurrencyStamp = Guid.Empty
        });
    }

    private static void SeedResourceCompanyAssignments(ModelBuilder modelBuilder)
    {
        // BERNARD -> société interne (SAFRAN) : coût contractuel/différentiel non applicables.
        // LEGRAND -> société externe (Externe Conseil) : coût contractuel/différentiel calculables.
        // GEORGES -> société interne (SAFRAN), en cohérence avec son historique TJM ci-dessus.
        modelBuilder.Entity<ResourceCompanyAssignment>().HasData(
            new ResourceCompanyAssignment
            {
                Id = SeedIds.AssignmentBernard, ResourceId = SeedIds.ResourceBernard, CompanyId = SeedIds.CompanySafran,
                StartDate = new DateOnly(2024, 1, 1), EndDate = null, AssignmentType = "Principale",
                Status = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            },
            new ResourceCompanyAssignment
            {
                Id = SeedIds.AssignmentLegrand, ResourceId = SeedIds.ResourceLegrand, CompanyId = SeedIds.CompanyExterneConseil,
                StartDate = new DateOnly(2024, 1, 1), EndDate = null, AssignmentType = "Principale",
                Status = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            },
            new ResourceCompanyAssignment
            {
                Id = SeedIds.AssignmentGeorges, ResourceId = SeedIds.ResourceGeorges, CompanyId = SeedIds.CompanySafran,
                StartDate = new DateOnly(2024, 1, 1), EndDate = null, AssignmentType = "Principale",
                Status = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor
            });
    }
}
