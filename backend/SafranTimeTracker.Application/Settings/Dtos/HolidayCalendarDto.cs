using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Settings.Dtos;

public class HolidayCalendarDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string Pays { get; set; } = string.Empty;
    public ReferentialStatus Statut { get; set; }
}

public class HolidayCalendarCreateRequest
{
    public DateOnly Date { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public string Pays { get; set; } = string.Empty;
}
