using System.Text.Json;

namespace NatarakiCarRental.Services;

public sealed class LocalAddressService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<AddressDataStore> DataStore = new(LoadAddressData, true);

    public Task<IReadOnlyList<PsgcRegionDto>> GetRegionsAsync()
    {
        IReadOnlyList<PsgcRegionDto> regions = DataStore.Value.Regions
            .OrderBy(region => region.Name)
            .ToList();

        return Task.FromResult(regions);
    }

    public Task<IReadOnlyList<PsgcProvinceDto>> GetProvincesByRegionAsync(string regionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionCode);

        IReadOnlyList<PsgcProvinceDto> provinces = DataStore.Value.Provinces
            .Where(province => string.Equals(province.RegionCode, regionCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(province => province.Name)
            .ToList();

        return Task.FromResult(provinces);
    }

    public Task<IReadOnlyList<PsgcCityMunicipalityDto>> GetCitiesByProvinceAsync(string provinceCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceCode);

        IReadOnlyList<PsgcCityMunicipalityDto> cities = DataStore.Value.Cities
            .Where(city => string.Equals(city.ProvinceCode, provinceCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(city => city.Name)
            .ToList();

        return Task.FromResult(cities);
    }

    public Task<IReadOnlyList<PsgcCityMunicipalityDto>> GetCitiesByRegionAsync(string regionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionCode);

        IReadOnlyList<PsgcCityMunicipalityDto> cities = DataStore.Value.Cities
            .Where(city => string.Equals(city.RegionCode, regionCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(city => city.Name)
            .ToList();

        return Task.FromResult(cities);
    }

    public Task<IReadOnlyList<PsgcBarangayDto>> GetBarangaysByCityAsync(string cityCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityCode);

        IReadOnlyList<PsgcBarangayDto> barangays = DataStore.Value.Barangays
            .Where(barangay =>
                string.Equals(barangay.CityCode, cityCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(barangay.MunicipalityCode, cityCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(barangay => barangay.Name)
            .ToList();

        return Task.FromResult(barangays);
    }

    private static AddressDataStore LoadAddressData()
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");
        string regionsPath = Path.Combine(dataDirectory, "regions.json");
        string provincesPath = Path.Combine(dataDirectory, "provinces.json");
        string citiesPath = Path.Combine(dataDirectory, "cities.json");
        string barangaysPath = Path.Combine(dataDirectory, "barangays.json");

        string[] requiredFiles = [regionsPath, provincesPath, citiesPath, barangaysPath];
        string[] missingFiles = requiredFiles.Where(path => !File.Exists(path)).ToArray();

        if (missingFiles.Length > 0)
        {
            throw new FileNotFoundException(
                $"Address database files not found. Place regions.json, provinces.json, cities.json, and barangays.json in '{dataDirectory}'.");
        }

        return new AddressDataStore(
            ReadJsonFile<PsgcRegionDto>(regionsPath),
            ReadJsonFile<PsgcProvinceDto>(provincesPath),
            ReadJsonFile<PsgcCityMunicipalityDto>(citiesPath),
            ReadJsonFile<PsgcBarangayDto>(barangaysPath));
    }

    private static IReadOnlyList<T> ReadJsonFile<T>(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Address database file '{Path.GetFileName(path)}' contains invalid JSON.", exception);
        }
    }

    private sealed record AddressDataStore(
        IReadOnlyList<PsgcRegionDto> Regions,
        IReadOnlyList<PsgcProvinceDto> Provinces,
        IReadOnlyList<PsgcCityMunicipalityDto> Cities,
        IReadOnlyList<PsgcBarangayDto> Barangays);
}
