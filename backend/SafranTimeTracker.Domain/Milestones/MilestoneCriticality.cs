namespace SafranTimeTracker.Domain.Milestones;

/// <summary>Cahier des charges §24.2. Valeurs non fixées par le cahier des charges ; mêmes valeurs
/// qu'ApplicationCriticality (Lot 1) par cohérence, sans les partager (concepts distincts).</summary>
public enum MilestoneCriticality
{
    Faible,
    Moyenne,
    Elevee,
    Critique
}
