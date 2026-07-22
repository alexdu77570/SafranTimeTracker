namespace SafranTimeTracker.Application.Common.Security;

/// <summary>
/// Paramètres externalisés de l'authentification simulée (CLAUDE.md §7 : "paramètres configurables
/// ... jamais en dur dans le code"), liés à la section <c>Authentication</c> de la configuration.
/// </summary>
public class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>Durée de vie glissante d'une session navigateur (<c>IsPersistent = false</c>).</summary>
    public int SessionLifetimeMinutes { get; set; } = 480;

    /// <summary>Durée de vie d'une session persistante ("se souvenir de moi", modèle prêt dès ce lot,
    /// fonctionnalité non exposée — voir <c>UserSession.IsPersistent</c>).</summary>
    public int PersistentSessionLifetimeDays { get; set; } = 30;

    /// <summary>Autorise la résolution directe de l'en-tête <c>X-Demo-User</c> sans session
    /// (façade de compatibilité des tests d'intégration existants) — <c>true</c> uniquement en
    /// Development/Test, jamais en Qualification/Production (CLAUDE.md §17).</summary>
    public bool AllowDirectDemoHeader { get; set; }
}
