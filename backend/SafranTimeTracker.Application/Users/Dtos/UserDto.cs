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
    public IReadOnlyList<Guid> PermissionIds { get; set; } = [];
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
