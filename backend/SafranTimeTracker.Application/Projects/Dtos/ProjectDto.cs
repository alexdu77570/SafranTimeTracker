using SafranTimeTracker.Domain.Projects;

namespace SafranTimeTracker.Application.Projects.Dtos;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public string? DescriptionCourte { get; set; }
    public Guid PiloteId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid StatusId { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinPrevueInitiale { get; set; }
    public DateOnly? DateFinAjustee { get; set; }
    public DateOnly? DateFinReelle { get; set; }
    public ProjectRiskLevel NiveauRisque { get; set; }
    public string? Commentaire { get; set; }

    /// <summary>Null sans FINANCIAL_DATA_VIEW (projection faite par ProjectService, CLAUDE.md §13).</summary>
    public ProjectFinancialSummaryDto? FinancialSummary { get; set; }
}

public class ProjectCreateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public string? DescriptionCourte { get; set; }
    public Guid PiloteId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? TeamId { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFinPrevueInitiale { get; set; }
    public decimal? BudgetInitial { get; set; }
    public ProjectRiskLevel NiveauRisque { get; set; } = ProjectRiskLevel.Faible;
    public string? Commentaire { get; set; }
}

/// <summary>Le statut n'est pas modifiable ici : archiver/réactiver sont des actions dédiées
/// (§16.3), pas une simple mise à jour de champ.</summary>
public class ProjectUpdateRequest
{
    public string Nom { get; set; } = string.Empty;
    public string? DescriptionCourte { get; set; }
    public Guid PiloteId { get; set; }
    public Guid? TeamId { get; set; }
    public DateOnly DateFinAjustee { get; set; }
    public DateOnly? DateFinReelle { get; set; }
    public decimal? BudgetInitial { get; set; }
    public ProjectRiskLevel NiveauRisque { get; set; }
    public string? Commentaire { get; set; }
}
