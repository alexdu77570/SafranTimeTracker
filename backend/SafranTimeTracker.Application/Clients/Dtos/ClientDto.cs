using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Clients.Dtos;

public class ClientDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
}

public class ClientCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Commentaire { get; set; }
}

/// <summary>Code (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class ClientUpdateRequest
{
    public string Nom { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
    public string? Commentaire { get; set; }
}
