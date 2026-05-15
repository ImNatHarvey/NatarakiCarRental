using System.Net.Http.Json;
using System.Text.Json;
using NatarakiCarRental.Services;

namespace NatarakiCarRental.Tools;

public static class AddressDataSeeder
{
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("https://psgc.gitlab.io/api/"),
        Timeout = TimeSpan.FromMinutes(2)
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task RunAsync(string outputDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        IReadOnlyList<PsgcRegionDto> regions = await DownloadAsync<PsgcRegionDto>("regions/", cancellationToken);
        IReadOnlyList<PsgcProvinceDto> provinces = await DownloadAsync<PsgcProvinceDto>("provinces/", cancellationToken);
        IReadOnlyList<PsgcCityMunicipalityDto> cities = await DownloadAsync<PsgcCityMunicipalityDto>("cities-municipalities/", cancellationToken);
        IReadOnlyList<PsgcBarangayDto> barangays = await DownloadAsync<PsgcBarangayDto>("barangays/", cancellationToken);

        await WriteJsonAsync(Path.Combine(outputDirectory, "regions.json"), regions, cancellationToken);
        await WriteJsonAsync(Path.Combine(outputDirectory, "provinces.json"), provinces, cancellationToken);
        await WriteJsonAsync(Path.Combine(outputDirectory, "cities.json"), cities, cancellationToken);
        await WriteJsonAsync(Path.Combine(outputDirectory, "barangays.json"), barangays, cancellationToken);
    }

    private static async Task<IReadOnlyList<T>> DownloadAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(relativeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken) ?? [];
    }

    private static async Task WriteJsonAsync<T>(string path, IReadOnlyList<T> items, CancellationToken cancellationToken)
    {
        await using FileStream stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, items, JsonOptions, cancellationToken);
    }
}
