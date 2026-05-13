using NatarakiCarRental.Models;

namespace NatarakiCarRental.Validators;

public sealed class CarValidator
{
    public IReadOnlyList<string> ValidateBasic(Car car)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(car.PlateNumber))
        {
            errors.Add("Plate number is required.");
        }

        if (string.IsNullOrWhiteSpace(car.Brand))
        {
            errors.Add("Brand is required.");
        }

        if (car.DailyRate < 0)
        {
            errors.Add("Daily rate cannot be negative.");
        }

        return errors;
    }
}
