namespace SafranTimeTracker.Application.Capacity.Dtos;

/// <summary>Résultat des calculs capacitaires (cahier des charges §29.1-29.4) pour une ressource
/// sur une période.</summary>
public class AvailabilityResultDto
{
    public Guid ResourceId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int JoursOuvres { get; set; }
    public int JoursFeries { get; set; }
    public decimal JoursAbsenceValidee { get; set; }

    /// <summary>§29.1 : joursOuvrés × capacitéJournalière (par jour, en tenant compte des
    /// variations de capacité).</summary>
    public decimal CapaciteTheorique { get; set; }

    /// <summary>§29.2 : capacitéThéorique - absencesValidées - joursFériés.</summary>
    public decimal CapaciteReelle { get; set; }

    /// <summary>§29.3, en pourcentage.</summary>
    public decimal TauxDisponibilite { get; set; }

    /// <summary>§29.4, en heures.</summary>
    public decimal ChargeRunHeures { get; set; }
    public decimal ChargeHorsRunHeures { get; set; }
}
