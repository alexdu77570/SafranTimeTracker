namespace SafranTimeTracker.Application.Common;

/// <summary>
/// Valeur de repli pour CreatedBy/UpdatedBy quand aucun appelant identifié n'est disponible
/// (Lot 1, avant l'introduction de <see cref="Security.ICurrentUser"/> au Lot 2 ; ou appel Lot 2+
/// sans en-tête de démonstration). Les services qui dépendent de <see cref="Security.ICurrentUser"/>
/// utilisent <c>ICurrentUser.Identifier</c>, qui retombe lui-même sur cette constante quand
/// <c>IsAuthenticated</c> est faux.
/// </summary>
public static class CurrentActor
{
    public const string PlaceholderIdentifier = "api-anonymous";
}
