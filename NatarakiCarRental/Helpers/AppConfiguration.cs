using System.Text.Json;

namespace NatarakiCarRental.Helpers;

public static class AppConfiguration
{
    private const string ConfigurationFileName = "appsettings.json";
    private static readonly Lazy<ConfigurationValues> Values = new(LoadValues);

    public static string? DatabaseServer => Values.Value.DatabaseServer;
    public static string? ConnectionString => Values.Value.ConnectionString;
    public static string? BootstrapOwnerUsername => Values.Value.BootstrapOwnerUsername;
    public static string? BootstrapOwnerPassword => Values.Value.BootstrapOwnerPassword;

    private static ConfigurationValues LoadValues()
    {
        string path = Path.Combine(AppContext.BaseDirectory, ConfigurationFileName);

        if (!File.Exists(path))
        {
            return new ConfigurationValues();
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;

            return new ConfigurationValues
            {
                DatabaseServer = GetString(root, "Database", "Server"),
                ConnectionString = GetString(root, "Database", "ConnectionString"),
                BootstrapOwnerUsername = GetString(root, "BootstrapOwner", "Username"),
                BootstrapOwnerPassword = GetString(root, "BootstrapOwner", "Password")
            };
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Configuration file '{ConfigurationFileName}' contains invalid JSON.",
                exception);
        }
    }

    private static string? GetString(JsonElement root, string sectionName, string propertyName)
    {
        if (!root.TryGetProperty(sectionName, out JsonElement section)
            || !section.TryGetProperty(propertyName, out JsonElement property)
            || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        string? value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed class ConfigurationValues
    {
        public string? DatabaseServer { get; init; }
        public string? ConnectionString { get; init; }
        public string? BootstrapOwnerUsername { get; init; }
        public string? BootstrapOwnerPassword { get; init; }
    }
}
