using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Users.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Identifiant { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public ReferentialStatus Statut { get; set; }
    public DateOnly DateArrivee { get; set; }
    public DateOnly? DateSortie { get; set; }
    public string? Commentaire { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid RoleId { get; set; }
    public bool AccesGlobal { get; set; }

    /// <summary>Exceptions individuelles à la matrice du rôle (octrois uniquement — les retraits
    /// individuels n'apparaissent pas ici, voir <see cref="EffectivePermissionCodes"/> pour le
    /// résultat consolidé). Conservé tel quel depuis le Lot 1 pour ne pas changer le contrat des
    /// écrans d'administration existants.</summary>
    public IReadOnlyList<Guid> PermissionIds { get; set; } = [];

    /// <summary>Permissions réellement actives pour cet utilisateur (rôle + exceptions
    /// individuelles, <c>PermissionResolutionService</c>, Lot 13) — seule source fiable pour piloter
    /// un affichage conditionnel côté frontend (<c>PermissionGuard</c>), jamais une barrière de
    /// sécurité en soi (CLAUDE.md §17).</summary>
    public IReadOnlyList<string> EffectivePermissionCodes { get; set; } = [];
}

public class UserCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Identifiant { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public DateOnly DateArrivee { get; set; }
    public string? Commentaire { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid RoleId { get; set; }
    public bool AccesGlobal { get; set; }
    public IReadOnlyList<Guid> PermissionIds { get; set; } = [];
}

/// <summary>Identifiant (clé métier), RoleId et permissions ne sont volontairement pas modifiables
/// ici (Lot 6) : le rôle passe par <see cref="RoleChangeRequest"/> (action dédiée, auditée
/// séparément — cahier des charges §28.3 "changement de rôle") et les permissions par les
/// endpoints d'octroi/retrait dédiés (§28.3 "changement de permission").</summary>
public class UserUpdateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public DateOnly DateArrivee { get; set; }
    public string? Commentaire { get; set; }
    public Guid? ResourceId { get; set; }
}

/// <summary>§28.3 "changement de rôle" / "promotion ou retrait Administrateur" : action dédiée,
/// motif facultatif, distincte d'une simple modification de fiche (CLAUDE.md §17 : garde-fou
/// dernier administrateur appliqué par <c>UserService</c>).</summary>
public class RoleChangeRequest
{
    public Guid RoleId { get; set; }
    public string? Motif { get; set; }
}
