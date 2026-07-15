namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Niveau de risque global d'un projet (cahier des charges §16.1, §16.2) — évaluation manuelle
/// portée par la fiche projet, distincte des risques planning/budget calculés (§29.5,
/// ProjectPlanningService). Valeurs non fixées par le cahier des charges ; choix raisonnable
/// (même principe qu'ApplicationCriticality, Lot 1).
/// </summary>
public enum ProjectRiskLevel
{
    Faible,
    Moyen,
    Eleve
}
