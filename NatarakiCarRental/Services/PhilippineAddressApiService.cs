using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;

namespace NatarakiCarRental.Services;

public sealed class PhilippineAddressApiService
{
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("https://psgc.gitlab.io/api/"),
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static IReadOnlyList<PsgcRegionDto>? _regions;
    private static readonly ConcurrentDictionary<string, IReadOnlyList<PsgcProvinceDto>> ProvincesByRegion = new();
    private static readonly ConcurrentDictionary<string, IReadOnlyList<PsgcCityMunicipalityDto>> CitiesByRegion = new();
    private static readonly ConcurrentDictionary<string, IReadOnlyList<PsgcCityMunicipalityDto>> CitiesByProvince = new();
    private static readonly ConcurrentDictionary<string, IReadOnlyList<PsgcBarangayDto>> BarangaysByCity = new();

    public async Task<IReadOnlyList<PsgcRegionDto>> GetRegionsAsync(CancellationToken cancellationToken = default)
    {
        if (_regions is not null)
        {
            return _regions;
        }

        IReadOnlyList<PsgcRegionDto> regions = await GetAsync<PsgcRegionDto>("regions/", cancellationToken);
        _regions = regions;
        return regions;
    }

    public async Task<IReadOnlyList<PsgcProvinceDto>> GetProvincesByRegionCodeAsync(
        string regionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionCode);

        if (ProvincesByRegion.TryGetValue(regionCode, out IReadOnlyList<PsgcProvinceDto>? provinces))
        {
            return provinces;
        }

        provinces = await GetAsync<PsgcProvinceDto>($"regions/{regionCode}/provinces/", cancellationToken);
        return ProvincesByRegion.GetOrAdd(regionCode, provinces);
    }

    public async Task<IReadOnlyList<PsgcCityMunicipalityDto>> GetCitiesMunicipalitiesByProvinceCodeAsync(
        string provinceCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceCode);

        if (CitiesByProvince.TryGetValue(provinceCode, out IReadOnlyList<PsgcCityMunicipalityDto>? cities))
        {
            return cities;
        }

        cities = await GetAsync<PsgcCityMunicipalityDto>($"provinces/{provinceCode}/cities-municipalities/", cancellationToken);
        return CitiesByProvince.GetOrAdd(provinceCode, cities);
    }

    public async Task<IReadOnlyList<PsgcCityMunicipalityDto>> GetCitiesMunicipalitiesByRegionCodeAsync(
        string regionCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionCode);

        if (CitiesByRegion.TryGetValue(regionCode, out IReadOnlyList<PsgcCityMunicipalityDto>? cities))
        {
            return cities;
        }

        cities = await GetAsync<PsgcCityMunicipalityDto>($"regions/{regionCode}/cities-municipalities/", cancellationToken);
        return CitiesByRegion.GetOrAdd(regionCode, cities);
    }

    public async Task<IReadOnlyList<PsgcBarangayDto>> GetBarangaysByCityMunicipalityCodeAsync(
        string cityMunicipalityCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityMunicipalityCode);

        if (BarangaysByCity.TryGetValue(cityMunicipalityCode, out IReadOnlyList<PsgcBarangayDto>? barangays))
        {
            return barangays;
        }

        barangays = await GetAsync<PsgcBarangayDto>($"cities-municipalities/{cityMunicipalityCode}/barangays/", cancellationToken);
        return BarangaysByCity.GetOrAdd(cityMunicipalityCode, barangays);
    }

    private static async Task<IReadOnlyList<T>> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await HttpClient.GetAsync(relativeUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            IReadOnlyList<T>? items = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
            return items ?? [];
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("The PSGC address service timed out.");
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException("Unable to reach the PSGC address service.", exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("The PSGC address service returned invalid data.", exception);
        }
    }
}

public sealed record PsgcRegionDto(
    string Code,
    string Name,
    string RegionName,
    string IslandGroupCode,
    string Psgc10DigitCode);

public sealed record PsgcProvinceDto(
    string Code,
    string Name,
    string RegionCode,
    string IslandGroupCode,
    string Psgc10DigitCode);

public sealed record PsgcCityMunicipalityDto(
    string Code,
    string Name,
    string ProvinceCode,
    string RegionCode,
    string IslandGroupCode,
    string Psgc10DigitCode);

public sealed record PsgcBarangayDto(
    string Code,
    string Name,
    string CityCode,
    string MunicipalityCode,
    string ProvinceCode,
    string RegionCode,
    string IslandGroupCode,
    string Psgc10DigitCode);
