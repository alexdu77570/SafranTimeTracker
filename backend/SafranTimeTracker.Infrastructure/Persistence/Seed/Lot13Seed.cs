using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Users;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Matrice de permissions par défaut du rôle applicatif (cahier des charges §6.1, Lot 13,
/// docs/BACKLOG_METIER.md §17 Décision 2) : dérivée des affectations individuelles déjà seedées
/// (Lots 1/5/6), sans en modifier une seule, pour garantir qu'aucun utilisateur seedé ne perd ou ne
/// gagne d'accès effectif à l'introduction de cette table.
/// - ADMINISTRATEUR : les 7 permissions existantes — Bernard (seul administrateur seedé) les
///   détient déjà toutes individuellement (Lots 1/5/6).
/// - RESPONSABLE_DEPARTEMENT, RESPONSABLE_SERVICE, INGENIEUR : aucune permission de rôle. Le seul
///   utilisateur RESPONSABLE_DEPARTEMENT (Legrand) ne détient que FINANCIAL_DATA_VIEW à titre
///   individuel (Lot 1) ; les quatre RESPONSABLE_SERVICE et sept INGENIEUR n'en détiennent aucune.
///   Généraliser FINANCIAL_DATA_VIEW à RESPONSABLE_DEPARTEMENT à partir d'un unique utilisateur
///   serait une règle métier inventée, non validée avec le Product Owner : l'accès de Legrand reste
///   une exception individuelle (UserPermission), pas une règle de rôle.
/// </summary>
internal static class Lot13Seed
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>().HasData(
            RolePermissionsFor(SeedIds.RoleAdministrateur,
                SeedIds.PermissionFinancialDataView,
                SeedIds.PermissionTimeEntryCorrection,
                SeedIds.PermissionUserAdministration,
                SeedIds.PermissionTimeEntryRecalculation,
                SeedIds.PermissionImportExecute,
                SeedIds.PermissionAuditView,
                SeedIds.PermissionOrderReceiptOverride));
    }

    private static RolePermission[] RolePermissionsFor(Guid roleId, params Guid[] permissionIds) =>
        permissionIds.Select(permissionId => new RolePermission { RoleId = roleId, PermissionId = permissionId }).ToArray();
}
