using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;

namespace SafranTimeTracker.Domain.Users;

/// <summary>
/// Compte autorisé à se connecter (cahier des charges §10.1, §10.2). Distinct de <see cref="Resource"/> :
/// une ressource peut exister sans compte actif, un compte peut exister sans ressource planifiée
/// (§10.1). Le lien est porté ici par <see cref="ResourceId"/> (nullable), jamais l'inverse.
/// </summary>
public class User : AuditableEntity
{
    // Informations générales (§10.2)
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Identifiant { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public ReferentialStatus Statut { get; set; } = ReferentialStatus.Actif;
    public DateOnly DateArrivee { get; set; }
    public DateOnly? DateSortie { get; set; }
    public string? Commentaire { get; set; }

    public Guid? ResourceId { get; set; }
    public Resource? Resource { get; set; }

    // Sécurité (§10.2)
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool AccesGlobal { get; set; }
    public DateTime? SecurityLastModifiedAt { get; set; }
    public string? SecurityLastModifiedBy { get; set; }

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
