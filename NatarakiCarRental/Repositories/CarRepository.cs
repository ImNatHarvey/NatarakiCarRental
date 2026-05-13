using Dapper;
using NatarakiCarRental.Data;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Repositories;

public sealed class CarRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CarRepository()
        : this(new DbConnectionFactory())
    {
    }

    public CarRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Car>> GetActiveCarsAsync()
    {
        return await SearchCarsAsync(string.Empty, includeArchived: false);
    }

    public async Task<IReadOnlyList<Car>> GetArchivedCarsAsync()
    {
        return await SearchCarsAsync(string.Empty, includeArchived: true);
    }

    public async Task<IReadOnlyList<Car>> SearchCarsAsync(string searchText, bool includeArchived)
    {
        const string sql = """
            SELECT
                CarId,
                CarName,
                Brand,
                Model,
                PlateNumber,
                [Year],
                Color,
                Transmission,
                FuelType,
                SeatingCapacity,
                RatePerDay,
                Status,
                ImagePath,
                OrCrPath,
                IsArchived,
                CreatedAt,
                UpdatedAt,
                ArchivedAt
            FROM dbo.Cars
            WHERE IsArchived = @IsArchived
              AND (
                    @SearchText = N''
                    OR CarName LIKE @SearchPattern
                    OR Model LIKE @SearchPattern
                    OR PlateNumber LIKE @SearchPattern
                  )
            ORDER BY CarId DESC;
            """;

        using var connection = _connectionFactory.CreateConnection();
        IEnumerable<Car> cars = await connection.QueryAsync<Car>(
            sql,
            new
            {
                IsArchived = includeArchived,
                SearchText = searchText.Trim(),
                SearchPattern = $"%{searchText.Trim()}%"
            });

        return cars.ToList();
    }

    public async Task<CarCounts> GetCarCountsAsync()
    {
        const string sql = """
            SELECT
                TotalCars = COUNT(CASE WHEN IsArchived = 0 THEN 1 END),
                AvailableCars = COUNT(CASE WHEN IsArchived = 0 AND Status = N'Available' THEN 1 END),
                RentedCars = COUNT(CASE WHEN IsArchived = 0 AND Status = N'Rented' THEN 1 END),
                ArchivedCars = COUNT(CASE WHEN IsArchived = 1 THEN 1 END)
            FROM dbo.Cars;
            """;

        using var connection = _connectionFactory.CreateConnection();
        CarCounts? counts = await connection.QuerySingleOrDefaultAsync<CarCounts>(sql);

        return counts ?? new CarCounts();
    }
}
