using SafranTimeTracker.Domain.Common;

namespace SafranTimeTracker.Application.Capacity.Dtos;

public class ResourceCapacityPeriodDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }
    public string? Reason { get; set; }
    public ReferentialStatus Status { get; set; }
}

public class ResourceCapacityPeriodCreateRequest
{
    public Guid ResourceId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal DailyCapacity { get; set; }
    public decimal WeeklyCapacity { get; set; }
    public string? Reason { get; set; }
}
