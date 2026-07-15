namespace SafranTimeTracker.Domain.Milestones;

/// <summary>
/// Cahier des charges §24.2. "En retard" n'est volontairement pas une valeur stockée ici : c'est
/// un état dérivé (DatePrevue dépassée et statut ni Terminé ni Annulé), calculé à la lecture par
/// MilestoneService pour éviter un job de rafraîchissement périodique.
/// </summary>
public enum MilestoneStatus
{
    AVenir,
    EnCours,
    Termine,
    Annule
}
