namespace NatarakiCarRental.Models;

public sealed class Car
{
    public int CarId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? Color { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int? SeatingCapacity { get; set; }
    public decimal RatePerDay { get; set; }
    public string Status { get; set; } = "Available";
    public string? CodingDay { get; set; }
    public int? Mileage { get; set; }
    public DateTime? RegistrationExpirationDate { get; set; }
    public DateTime? InsuranceExpirationDate { get; set; }
    public string? ImagePath { get; set; }
    public string? OrCrPath { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public int Id
    {
        get => CarId;
        set => CarId = value;
    }

    public decimal DailyRate
    {
        get => RatePerDay;
        set => RatePerDay = value;
    }
}
