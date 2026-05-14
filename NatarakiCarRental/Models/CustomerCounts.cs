namespace NatarakiCarRental.Models;

public sealed class CustomerCounts
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int BlacklistedCustomers { get; set; }
    public int ArchivedCustomers { get; set; }
}
