namespace SafranTimeTracker.Domain.Projects;

/// <summary>Actif, Suspendu, Terminé, Archivé (cahier des charges §16.2, §30).</summary>
public class ProjectStatus
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public int Ordre { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
