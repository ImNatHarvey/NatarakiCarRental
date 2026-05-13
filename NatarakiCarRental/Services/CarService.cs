using FluentValidation;
using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;
using NatarakiCarRental.Validators;

namespace NatarakiCarRental.Services;

public sealed class CarService
{
    private readonly CarRepository _carRepository;

    public CarService()
        : this(new CarRepository())
    {
    }

    public CarService(CarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public Task<IReadOnlyList<Car>> GetActiveCarsAsync()
    {
        return _carRepository.GetActiveCarsAsync();
    }

    public Task<IReadOnlyList<Car>> GetArchivedCarsAsync()
    {
        return _carRepository.GetArchivedCarsAsync();
    }

    public Task<IReadOnlyList<Car>> SearchCarsAsync(string searchText, bool includeArchived)
    {
        return _carRepository.SearchCarsAsync(searchText, includeArchived);
    }

    public Task<CarCounts> GetCarCountsAsync()
    {
        return _carRepository.GetCarCountsAsync();
    }

    public async Task<int> AddCarAsync(Car car)
    {
        car.CarName = car.CarName.Trim();
        car.Brand = car.Brand.Trim();
        car.Model = car.Model.Trim();
        car.PlateNumber = car.PlateNumber.Trim().ToUpperInvariant();
        car.Color = car.Color?.Trim();
        car.Transmission = car.Transmission?.Trim();
        car.FuelType = car.FuelType?.Trim();
        car.ImagePath = car.ImagePath?.Trim();
        car.OrCrPath = car.OrCrPath?.Trim();

        CarValidator validator = new();
        validator.ValidateAndThrow(car);

        bool plateExists = await _carRepository.PlateNumberExistsAsync(car.PlateNumber);

        if (plateExists)
        {
            throw new ValidationException("Plate number already exists.");
        }

        return await _carRepository.AddCarAsync(car);
    }
}
