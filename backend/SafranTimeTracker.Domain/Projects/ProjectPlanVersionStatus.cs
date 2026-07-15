namespace SafranTimeTracker.Domain.Projects;

/// <summary>
/// Précision actée avec l'utilisateur (Lot 4) : une seule version Ajustée peut être Active à la
/// fois ; les précédentes basculent automatiquement à Archivee quand une nouvelle devient Active
/// (ProjectPlanningService), sans jamais être supprimées. Une version Initiale reste Active en
/// permanence (immuable, jamais archivée : il n'en existe qu'une par projet).
/// </summary>
public enum ProjectPlanVersionStatus
{
    Active,
    Archivee
}
