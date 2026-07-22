namespace SafranTimeTracker.Application.Auth.Dtos;

/// <summary>
/// Création d'une session simulée (Lot 13). <see cref="RememberMe"/> existe dès ce lot dans le
/// contrat pour que le modèle de session (<c>UserSession.IsPersistent</c>) n'ait jamais besoin
/// d'être retouché quand la fonctionnalité "se souvenir de moi" sera exposée à l'écran — aucune
/// interface ne l'envoie encore à <c>true</c>.
/// </summary>
public class AuthSessionRequest
{
    public string Identifiant { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class AuthSessionDto
{
    public Guid UserId { get; set; }
    public string Identifiant { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsPersistent { get; set; }
}
