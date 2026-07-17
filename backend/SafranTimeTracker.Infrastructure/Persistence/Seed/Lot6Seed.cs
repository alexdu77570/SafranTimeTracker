using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Auditing;
using SafranTimeTracker.Domain.Imports;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 6 : les 5 nouvelles permissions (§6.2 — administration
/// utilisateur, recalcul financier, exécution d'import, consultation d'audit, dépassement de
/// réception de commande), toutes accordées à Bernard (seul Administrateur du jeu de données,
/// Lot 1) ; une réception partielle sur la commande de démonstration (règle métier validée,
/// §13) ; un lot d'import confirmé avec ses diffs (§27.5-27.6) ; deux entrées d'audit illustratives
/// (§28.3). Idempotent (HasData), dates/montants strictement déterministes.
/// </summary>
internal static class Lot6Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedPermissions(modelBuilder);
        SeedOrderReceipt(modelBuilder);
        SeedImportBatch(modelBuilder);
        SeedAuditLogs(modelBuilder);
    }

    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                Id = SeedIds.PermissionUserAdministration,
                Code = "USER_ADMINISTRATION",
                Libelle = "Administration des utilisateurs",
                Description = "Autorise la modification/désactivation d'un utilisateur, le changement de rôle et l'octroi/retrait de permission (cahier des charges §28.3)."
            },
            new Permission
            {
                Id = SeedIds.PermissionTimeEntryRecalculation,
                Code = "TIME_ENTRY_RECALCULATION",
                Libelle = "Recalcul financier explicite d'une saisie",
                Description = "Autorise le recalcul explicite d'une saisie déjà valorisée (cahier des charges §19.6)."
            },
            new Permission
            {
                Id = SeedIds.PermissionImportExecute,
                Code = "IMPORT_EXECUTE",
                Libelle = "Exécution d'un import",
                Description = "Autorise l'aperçu, la simulation et l'exécution d'un import (cahier des charges §27)."
            },
            new Permission
            {
                Id = SeedIds.PermissionAuditView,
                Code = "AUDIT_VIEW",
                Libelle = "Consultation du journal d'audit",
                Description = "Autorise la consultation du journal d'audit (cahier des charges §28.1)."
            },
            new Permission
            {
                Id = SeedIds.PermissionOrderReceiptOverride,
                Code = "ORDER_RECEIPT_OVERRIDE",
                Libelle = "Dépassement du reste réceptionnable d'une commande",
                Description = "Autorise l'enregistrement d'une réception de commande au-delà du reste réceptionnable (règle métier validée, Lot 6)."
            });

        modelBuilder.Entity<UserPermission>().HasData(
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionUserAdministration, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor },
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionTimeEntryRecalculation, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor },
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionImportExecute, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor },
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionAuditView, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor },
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionOrderReceiptOverride, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor });
    }

    /// <summary>Réception partielle sur la commande de démonstration (§13) : le vocabulaire
    /// "Réceptions partielles" se matérialise par cet événement, sans toucher au statut de la
    /// commande (machine d'état inchangée depuis le Lot 5).</summary>
    private static void SeedOrderReceipt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderReceipt>().HasData(new OrderReceipt
        {
            Id = SeedIds.OrderReceiptDemo,
            OrderId = SeedIds.OrderDemo,
            ReceiptDate = new DateOnly(2026, 3, 1),
            ReceivedAmount = 15000.00m,
            Reason = "Première réception partielle (démonstration).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    /// <summary>Lot d'import confirmé de démonstration (§27.5-27.6) : illustre un ajout et une
    /// modification sur le référentiel Applications, sans qu'aucune donnée réelle n'ait été
    /// modifiée par ce seed (les diffs sont purement illustratifs, aucune entité Application n'est
    /// créée/modifiée en conséquence).</summary>
    private static void SeedImportBatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportBatch>().HasData(new ImportBatch
        {
            Id = SeedIds.ImportBatchDemo,
            Type = ImportEntityType.Applications,
            Source = "CSV",
            ImportDate = SeedTimestamp,
            UserId = SeedAuthor,
            Mode = ImportMode.MiseAJour,
            FileName = "applications-demo.csv",
            LineCount = 2,
            AddCount = 1,
            UpdateCount = 1,
            DeleteCount = 0,
            ErrorCount = 0,
            Status = ImportBatchStatus.Confirme,
            Checksum = "demo-checksum-lot6"
        });

        modelBuilder.Entity<ImportDiff>().HasData(
            new ImportDiff
            {
                Id = SeedIds.ImportDiffDemoAdd,
                ImportBatchId = SeedIds.ImportBatchDemo,
                EntityType = nameof(ImportEntityType.Applications),
                EntityId = SeedIds.AppServiceNow,
                DiffType = ImportDiffType.Ajout
            },
            new ImportDiff
            {
                Id = SeedIds.ImportDiffDemoUpdate,
                ImportBatchId = SeedIds.ImportBatchDemo,
                EntityType = nameof(ImportEntityType.Applications),
                EntityId = SeedIds.AppVtom,
                DiffType = ImportDiffType.Modification,
                FieldName = "Criticite",
                OldValue = "Moyenne",
                NewValue = "Haute"
            });
    }

    private static void SeedAuditLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>().HasData(
            new AuditLog
            {
                Id = SeedIds.AuditLogDemoOrderExtension,
                Author = SeedAuthor,
                Timestamp = SeedTimestamp,
                Action = "EXTENSION",
                EntityType = nameof(Order),
                EntityId = SeedIds.OrderDemo,
                OldValue = "{\"BudgetFinancierAjuste\":150000.00}",
                NewValue = "{\"BudgetFinancierAjuste\":170000.00}",
                Reason = "Extension de périmètre validée par le comité de pilotage (démonstration, cf. OrderExtensionDemo)."
            },
            new AuditLog
            {
                Id = SeedIds.AuditLogDemoImport,
                Author = SeedAuthor,
                Timestamp = SeedTimestamp,
                Action = "IMPORT",
                EntityType = nameof(ImportEntityType.Applications),
                EntityId = SeedIds.ImportBatchDemo,
                NewValue = "{\"Mode\":\"MiseAJour\",\"AddCount\":1,\"UpdateCount\":1}"
            });
    }
}
