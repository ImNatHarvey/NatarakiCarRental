using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;

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
}
