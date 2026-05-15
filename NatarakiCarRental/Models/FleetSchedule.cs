namespace NatarakiCarRental.Models;

public sealed class FleetSchedule
{
    public int ScheduleId { get; set; }
    public int CarId { get; set; }
    public int? CustomerId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsArchived { get; set; }
}
