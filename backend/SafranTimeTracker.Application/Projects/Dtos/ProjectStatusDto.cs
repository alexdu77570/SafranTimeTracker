namespace SafranTimeTracker.Application.Projects.Dtos;

public class ProjectStatusDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public int Ordre { get; set; }
}
