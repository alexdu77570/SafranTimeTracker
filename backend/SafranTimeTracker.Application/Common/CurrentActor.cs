namespace SafranTimeTracker.Application.Common;

/// <summary>
/// Espace réservé tant qu'aucune authentification n'existe (différée après le Lot 0, voir
/// docs/IMPLEMENTATION_STATUS.md). Utilisé comme valeur CreatedBy/UpdatedBy le temps qu'un
/// contexte utilisateur réel soit disponible. À remplacer par l'identité authentifiée dès que
/// l'authentification sera implémentée — ne pas construire de mécanisme d'identité anticipé ici.
/// </summary>
public static class CurrentActor
{
    public const string PlaceholderIdentifier = "api-anonymous";
}
