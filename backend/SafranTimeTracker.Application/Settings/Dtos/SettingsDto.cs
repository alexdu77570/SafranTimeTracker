namespace SafranTimeTracker.Application.Settings.Dtos;

/// <summary>
/// Ligne singleton (cahier des charges §28.2). HolidayCalendar, ActivityType, AbsenceType et
/// MilestoneType sont des entités distinctes différées à leurs lots respectifs (3 et 4).
/// </summary>
public class SettingsDto
{
    public decimal HeuresParJour { get; set; }
    public int JoursOuvresParSemaine { get; set; }
    public string PaysParDefaut { get; set; } = string.Empty;
    public string DeviseParDefaut { get; set; } = string.Empty;
    public decimal? SeuilSurcharge { get; set; }
    public decimal? SeuilSousCharge { get; set; }
    public decimal? SeuilAlerteBudget { get; set; }
    public decimal? SeuilAlerteCommande { get; set; }
    public int DelaiModificationTempsJours { get; set; }
    public bool ActivationValidationAbsences { get; set; }
    public bool AutorisationSaisieSansValorisation { get; set; }
}

public class SettingsUpdateRequest
{
    public decimal HeuresParJour { get; set; }
    public int JoursOuvresParSemaine { get; set; }
    public string PaysParDefaut { get; set; } = string.Empty;
    public string DeviseParDefaut { get; set; } = string.Empty;
    public decimal? SeuilSurcharge { get; set; }
    public decimal? SeuilSousCharge { get; set; }
    public decimal? SeuilAlerteBudget { get; set; }
    public decimal? SeuilAlerteCommande { get; set; }
    public int DelaiModificationTempsJours { get; set; }
    public bool ActivationValidationAbsences { get; set; }
    public bool AutorisationSaisieSansValorisation { get; set; }
}
