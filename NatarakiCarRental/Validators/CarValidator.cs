using FluentValidation;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Validators;

public sealed class CarValidator : AbstractValidator<Car>
{
    private static readonly string[] ValidStatuses = ["Available", "Rented", "Under Maintenance"];
    private static readonly string[] ValidTransmissions = ["Automatic", "Manual", "CVT"];
    private static readonly string[] ValidFuelTypes = ["Gasoline", "Diesel", "Hybrid", "Electric"];
    private static readonly string[] ValidCodingDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "None / Not Applicable"];

    public CarValidator()
    {
        RuleFor(car => car.CarName)
            .NotEmpty()
            .WithMessage("Car name is required.");

        RuleFor(car => car.Model)
            .NotEmpty()
            .WithMessage("Model is required.");

        RuleFor(car => car.PlateNumber)
            .NotEmpty()
            .WithMessage("Plate number is required.");

        RuleFor(car => car.PlateNumber)
            .Must(value => string.IsNullOrWhiteSpace(value) || value == value.ToUpperInvariant())
            .WithMessage("Plate number must be uppercase.");

        RuleFor(car => car.RatePerDay)
            .GreaterThan(0)
            .WithMessage("Rate per day must be greater than 0.");

        RuleFor(car => car.Year)
            .InclusiveBetween(1000, 9999)
            .When(car => car.Year.HasValue)
            .WithMessage("Year must be 4 digits.");

        RuleFor(car => car.SeatingCapacity)
            .GreaterThan(0)
            .When(car => car.SeatingCapacity.HasValue)
            .WithMessage("Seating capacity must be greater than 0.");

        RuleFor(car => car.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(car => car.Mileage.HasValue)
            .WithMessage("Mileage cannot be a negative number.");

        RuleFor(car => car.Status)
            .NotEmpty()
            .WithMessage("Status is required.");

        RuleFor(car => car.Status)
            .Must(status => ValidStatuses.Contains(status))
            .WithMessage("Status must be Available, Rented, or Under Maintenance.");

        RuleFor(car => car.Transmission)
            .Must(value => string.IsNullOrWhiteSpace(value) || ValidTransmissions.Contains(value))
            .WithMessage("Transmission must be Automatic, Manual, or CVT.");

        RuleFor(car => car.FuelType)
            .Must(value => string.IsNullOrWhiteSpace(value) || ValidFuelTypes.Contains(value))
            .WithMessage("Fuel type must be Gasoline, Diesel, Hybrid, or Electric.");

        RuleFor(car => car.CodingDay)
            .Must(value => string.IsNullOrWhiteSpace(value) || ValidCodingDays.Contains(value))
            .WithMessage("Car coding day must be Monday, Tuesday, Wednesday, Thursday, Friday, or None / Not Applicable.");
    }
}
