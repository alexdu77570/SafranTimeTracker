namespace SafranTimeTracker.Domain.Settings;

/// <summary>
/// Paramètres globaux de l'application (cahier des charges §28.2). Ligne singleton (une seule
/// instance en base). Seuls les paramètres directement exploitables sans fonctionnalité
/// dépendante sont repris ici ; HolidayCalendar, ActivityType, AbsenceType et MilestoneType
/// sont des entités distinctes du §30, différées à leurs lots respectifs (3 et 4).
/// </summary>
public class Settings
{
    public Guid Id { get; set; }

    public decimal HeuresParJour { get; set; } = 7.75m;
    public int JoursOuvresParSemaine { get; set; } = 5;
    public string PaysParDefaut { get; set; } = "France";
    public string DeviseParDefaut { get; set; } = "EUR";

    public decimal? SeuilSurcharge { get; set; }
    public decimal? SeuilSousCharge { get; set; }
    public decimal? SeuilAlerteBudget { get; set; }
    public decimal? SeuilAlerteCommande { get; set; }

    public int DelaiModificationTempsJours { get; set; }
    public bool ActivationValidationAbsences { get; set; }
    public bool AutorisationSaisieSansValorisation { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
