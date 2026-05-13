using FluentValidation;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Validators;

public sealed class CarValidator : AbstractValidator<Car>
{
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

        RuleFor(car => car.Status)
            .NotEmpty()
            .WithMessage("Status is required.");
    }
}
