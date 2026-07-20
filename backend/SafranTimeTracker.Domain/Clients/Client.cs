using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Domain.Clients;

/// <summary>
/// Référentiel des clients (docs/BACKLOG_METIER.md §6, Lot 8) : donneur d'ordre/bénéficiaire d'un
/// projet, distinct de <see cref="Companies.Company"/> qui reste exclusivement la société
/// prestataire fournissant la ressource et son TJM contractuel. Aucune incidence financière.
/// </summary>
public class Client : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public string? Commentaire { get; set; }
}
