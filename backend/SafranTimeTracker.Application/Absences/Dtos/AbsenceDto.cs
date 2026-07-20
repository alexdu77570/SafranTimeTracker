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

/// <summary>
/// Modification d'un brouillon (§23.2 "modifier tant que permis", docs/BACKLOG_METIER.md §12) :
/// ResourceId n'est volontairement pas modifiable, même convention que TimeEntryUpdateRequest —
/// changer le titulaire d'une demande est une opération différente (annuler puis recréer).
/// Restreint au statut Brouillon par AbsenceService.UpdateAsync (409 sinon).
/// </summary>
public class AbsenceUpdateRequest
{
    public AbsenceType Type { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
    public bool DemiJournee { get; set; }
    public string? Commentaire { get; set; }
}
