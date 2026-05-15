namespace NatarakiCarRental.Helpers;

public static class FleetScheduleConstants
{
    public static class Type
    {
        public const string Reservation = "Reservation";
        public const string Rental = "Rental";
        public const string Maintenance = "Maintenance";
        public const string Blocked = "Blocked";

        public static readonly string[] All = [Reservation, Rental, Maintenance, Blocked];
    }

    public static class Status
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All = [Pending, Confirmed, Active, Completed, Cancelled];
    }
}
