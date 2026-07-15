namespace SafranTimeTracker.Domain.Companies;

/// <summary>Interne ou Externe (cahier des charges §12.1, §30). Table de référence.</summary>
public class CompanyType
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;

    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
