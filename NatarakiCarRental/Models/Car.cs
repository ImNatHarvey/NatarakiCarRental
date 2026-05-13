namespace NatarakiCarRental.Models;

public sealed class Car
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "Available";
    public string? ImagePath { get; set; }
    public bool IsArchived { get; set; }
}
