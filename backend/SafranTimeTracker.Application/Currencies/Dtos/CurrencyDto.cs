using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Currencies.Dtos;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string CodeIso { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Symbole { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}

public class CurrencyCreateRequest
{
    public string CodeIso { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Symbole { get; set; } = string.Empty;
}

/// <summary>Code ISO (clé métier) volontairement non modifiable, même convention que Company.Code (CLAUDE.md §5).</summary>
public class CurrencyUpdateRequest
{
    public string Libelle { get; set; } = string.Empty;
    public string Symbole { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}
