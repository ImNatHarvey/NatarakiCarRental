namespace NatarakiCarRental.Models;

public sealed class ActivityLogMetrics
{
    public int TotalLogs { get; set; }
    public int TodaysLogs { get; set; }
    public int CarActions { get; set; }
    public int CustomerActions { get; set; }
}
