namespace NatarakiCarRental.Services;

public sealed class ActivityLogService
{
    public void LogPlaceholder(string action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
    }
}
