using SafranTimeTracker.Domain.Budgets;

namespace SafranTimeTracker.Application.Budgets.Dtos;

/// <summary>
/// Ressource intégralement financière (cahier des charges §14) : contrairement à
/// <c>ProjectDto</c>/<c>OrderDto</c>, aucun champ n'est omis à la pièce — l'endpoint entier est
/// gardé par FINANCIAL_DATA_VIEW au niveau contrôleur (403 sans permission, même principe que les
/// endpoints du Lot 2 : RequirePermissionAttribute).
/// </summary>
public class BudgetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? OrderId { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public BudgetStatus Status { get; set; }
    public decimal? AlertThreshold { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Comment { get; set; }

    public decimal CoutReelConsomme { get; set; }
    public decimal CoutContractuelConsomme { get; set; }
    public decimal Differentiel { get; set; }
    public decimal MontantRestant { get; set; }

    /// <summary>Simplification MVP validée : égal au coût réel déjà consommé, faute de plan de
    /// charge associé à un budget générique (contrairement à un projet, §29.5) — pas de modèle
    /// prédictif avancé.</summary>
    public decimal AtterrissageEstime { get; set; }
    public bool RisqueDepassement { get; set; }
}

public class BudgetCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? OrderId { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal? AlertThreshold { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Comment { get; set; }
}

/// <summary>Montant initial et rattachements (projet/commande) ne sont pas modifiables ici :
/// AdjustedAmount évolue exclusivement via <see cref="BudgetAdjustRequest"/> (§14.2, historisé).</summary>
public class BudgetUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal? AlertThreshold { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Comment { get; set; }
}
