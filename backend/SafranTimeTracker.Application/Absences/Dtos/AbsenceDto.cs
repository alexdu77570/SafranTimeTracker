using SafranTimeTracker.Domain.Absences;

namespace SafranTimeTracker.Application.Absences.Dtos;

public class AbsenceDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public AbsenceType Type { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public bool DemiJournee { get; set; }
    public string? Commentaire { get; set; }
    public AbsenceStatus Statut { get; set; }
    public string? ValideParIdentifiant { get; set; }
    public DateTime? DateDecision { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class AbsenceCreateRequest
{
    public Guid ResourceId { get; set; }
    public AbsenceType Type { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public bool DemiJournee { get; set; }
    public string? Commentaire { get; set; }
}

/// <summary>Motif obligatoire uniquement en cas de refus (cahier des charges §23.3).</summary>
public class AbsenceDecisionRequest
{
    public string? Commentaire { get; set; }
}
