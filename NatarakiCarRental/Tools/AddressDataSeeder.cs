using System.Net.Http.Headers;
using System.Text.Json;

namespace NatarakiCarRental.Tools;

public static class AddressDataSeeder
{
    private static readonly IReadOnlyList<AddressDataFile> RequiredFiles =
    [
        new("regions.json", "regions"),
        new("provinces.json", "provinces"),
        new("cities.json", "cities-municipalities"),
        new("barangays.json", "barangays")
    ];

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");
        Directory.CreateDirectory(dataDirectory);

        foreach (AddressDataFile file in RequiredFiles)
        {
            string targetPath = Path.Combine(dataDirectory, file.FileName);

            if (IsValidAddressFile(targetPath))
            {
                continue;
            }

            await DownloadAndWriteAsync(file, targetPath, cancellationToken);
        }

        VerifyAllFiles(dataDirectory);
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri("https://psgc.gitlab.io/api/"),
            Timeout = TimeSpan.FromSeconds(60)
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NatarakiCarRental", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static async Task DownloadAndWriteAsync(
        AddressDataFile file,
        string targetPath,
        CancellationToken cancellationToken)
    {
        string temporaryPath = $"{targetPath}.tmp";

        try
        {
            using HttpResponseMessage response = await HttpClient.GetAsync(file.Endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to download '{file.FileName}' from PSGC endpoint '{file.Endpoint}'. " +
                    $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);
            ValidateJsonArray(json, file.FileName);

            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
            File.Move(temporaryPath, targetPath, overwrite: true);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }

    private static bool IsValidAddressFile(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            ValidateJsonArray(json, Path.GetFileName(path));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void VerifyAllFiles(string dataDirectory)
    {
        foreach (AddressDataFile file in RequiredFiles)
        {
            string path = Path.Combine(dataDirectory, file.FileName);

            if (!IsValidAddressFile(path))
            {
                throw new InvalidOperationException(
                    $"Address data file '{file.FileName}' is missing or invalid after seeding.");
            }
        }
    }

    private static void ValidateJsonArray(string? json, string fileName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException($"Downloaded address data for '{fileName}' is empty.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    $"Address data file '{fileName}' must contain a JSON array.");
            }

            if (document.RootElement.GetArrayLength() == 0)
            {
                throw new InvalidOperationException(
                    $"Address data file '{fileName}' contains an empty JSON array.");
            }
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Address data file '{fileName}' contains invalid JSON.",
                exception);
        }
    }

    private sealed record AddressDataFile(string FileName, string Endpoint);
}
