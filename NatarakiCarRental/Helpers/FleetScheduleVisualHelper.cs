using System.Drawing;

namespace NatarakiCarRental.Helpers;

public static class FleetScheduleVisualHelper
{
    private static readonly Color MaintenanceColor = Color.FromArgb(234, 88, 12);
    private static readonly Color CompletedColor = Color.FromArgb(71, 85, 105);

    public static IReadOnlyList<StatusDisplayOption> StatusOptions { get; } =
    [
        new(FleetScheduleConstants.Status.Pending, "Pending"),
        new(FleetScheduleConstants.Status.Confirmed, "Reserved"),
        new(FleetScheduleConstants.Status.Active, "Rented"),
        new(FleetScheduleConstants.Status.Completed, "Completed"),
        new(FleetScheduleConstants.Status.Cancelled, "Cancelled")
    ];

    public static string GetDisplayStatus(string status, string? scheduleType = null)
    {
        if (scheduleType == FleetScheduleConstants.Type.Maintenance
            && status != FleetScheduleConstants.Status.Cancelled
            && status != FleetScheduleConstants.Status.Completed)
        {
            return "Maintenance";
        }

        return StatusOptions.FirstOrDefault(option => option.Value == status)?.Label ?? status;
    }

    public static Color GetColor(string status, string? scheduleType = null)
    {
        if (status == FleetScheduleConstants.Status.Cancelled)
        {
            return ThemeHelper.GrayIcon;
        }

        if (status == FleetScheduleConstants.Status.Completed)
        {
            return CompletedColor;
        }

        if (scheduleType == FleetScheduleConstants.Type.Maintenance)
        {
            return MaintenanceColor;
        }

        return status switch
        {
            FleetScheduleConstants.Status.Pending => ThemeHelper.Warning,
            FleetScheduleConstants.Status.Confirmed => ThemeHelper.Primary,
            FleetScheduleConstants.Status.Active => ThemeHelper.Success,
            _ => ThemeHelper.Purple
        };
    }

    public sealed record StatusDisplayOption(string Value, string Label)
    {
        public override string ToString() => Label;
    }
}
