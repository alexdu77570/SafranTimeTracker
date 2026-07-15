namespace SafranTimeTracker.Domain.Applications;

/// <summary>
/// Niveau de criticité d'une application. Le cahier des charges §15.2 ne définit pas de valeurs
/// fermées ; celles-ci sont un choix raisonnable et pourront être ajustées.
/// </summary>
public enum ApplicationCriticality
{
    Faible,
    Moyenne,
    Elevee,
    Critique
}
