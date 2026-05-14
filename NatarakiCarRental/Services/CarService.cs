using FluentValidation;
using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;
using NatarakiCarRental.Validators;

namespace NatarakiCarRental.Services;

public sealed class CarService
{
    private readonly CarRepository _carRepository;
    private readonly ActivityLogService _activityLogService;

    public CarService()
        : this(new CarRepository(), new ActivityLogService())
    {
    }

    public CarService(CarRepository carRepository, ActivityLogService activityLogService)
    {
        _carRepository = carRepository;
        _activityLogService = activityLogService;
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

    public Task<Car?> GetCarByIdAsync(int carId)
    {
        return _carRepository.GetCarByIdAsync(carId);
    }

    public Task<bool> PlateNumberExistsAsync(string plateNumber, int? excludingCarId = null)
    {
        return _carRepository.PlateNumberExistsAsync(plateNumber, excludingCarId);
    }

    public async Task<int> AddCarAsync(Car car)
    {
        NormalizeCar(car);

        CarValidator validator = new();
        validator.ValidateAndThrow(car);

        bool plateExists = await _carRepository.PlateNumberExistsAsync(car.PlateNumber);

        if (plateExists)
        {
            throw new ValidationException("Plate number already exists.");
        }

        int carId = await _carRepository.AddCarAsync(car);
        await _activityLogService.LogAsync(
            "Add car",
            "Car",
            carId,
            $"Added car {car.CarName} ({car.PlateNumber}).");

        return carId;
    }

    public async Task UpdateCarAsync(Car car)
    {
        NormalizeCar(car);

        CarValidator validator = new();
        validator.ValidateAndThrow(car);

        bool plateExists = await _carRepository.PlateNumberExistsAsync(car.PlateNumber, car.CarId);

        if (plateExists)
        {
            throw new ValidationException("Plate number already exists.");
        }

        await _carRepository.UpdateCarAsync(car);
        await _activityLogService.LogAsync(
            "Edit car",
            "Car",
            car.CarId,
            $"Edited car {car.CarName} ({car.PlateNumber}).");
    }

    public async Task ArchiveCarAsync(int carId)
    {
        Car? car = await _carRepository.GetCarByIdAsync(carId);
        await _carRepository.ArchiveCarAsync(carId);
        await _activityLogService.LogAsync(
            "Archive car",
            "Car",
            carId,
            $"Archived car {DescribeCar(car, carId)}.");
    }

    public async Task RestoreCarAsync(int carId)
    {
        Car? car = await _carRepository.GetCarByIdAsync(carId);
        await _carRepository.RestoreCarAsync(carId);
        await _activityLogService.LogAsync(
            "Restore car",
            "Car",
            carId,
            $"Restored car {DescribeCar(car, carId)}.");
    }

    private static void NormalizeCar(Car car)
    {
        car.CarName = car.CarName.Trim();
        car.Brand = car.Brand.Trim();
        car.Model = car.Model.Trim();
        car.PlateNumber = car.PlateNumber.Trim().ToUpperInvariant();
        car.Color = NullIfWhiteSpace(car.Color);
        car.Transmission = NullIfWhiteSpace(car.Transmission);
        car.FuelType = NullIfWhiteSpace(car.FuelType);
        car.CodingDay = NullIfWhiteSpace(car.CodingDay);
        car.ImagePath = NullIfWhiteSpace(car.ImagePath);
        car.OrCrPath = NullIfWhiteSpace(car.OrCrPath);
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string DescribeCar(Car? car, int carId)
    {
        return car is null ? $"#{carId}" : $"{car.CarName} ({car.PlateNumber})";
    }
}
