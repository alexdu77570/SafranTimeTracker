namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Effet d'une permission individuelle sur le calcul RBAC (cahier des charges §6.1, Lot 13) :
/// une ligne <see cref="UserPermission"/> complète (<see cref="Grant"/>) ou retire
/// (<see cref="Revoke"/>) une permission par rapport à ce que le rôle de l'utilisateur accorde déjà
/// (<see cref="RolePermission"/>). <see cref="Grant"/> vaut 0 pour que les lignes historiques
/// (Lots 1, 5, 6), créées avant l'introduction de cette colonne, restent des octrois sans migration
/// de données.
/// </summary>
public enum UserPermissionEffect
{
    Grant = 0,
    Revoke = 1,
}
