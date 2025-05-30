using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AzureDailyCostCompare.Infrastructure;

public static class ConfigurationManager
{
    public const string ConfigFileName = "appsettings.json";

    public static IConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: false);

        return builder.Build();
    }

    public static void ValidatePreviousDayUtcDataLoadDelayHours(IConfiguration configuration)
    {
        if (!configuration.GetSection("AppSettings:PreviousDayUtcDataLoadDelayHours:Value").Exists())
        {
            throw new ConfigurationValidationException("Missing required configuration value: AppSettings:PreviousDayUtcDataLoadDelayHours:Value");
        }

        if (!int.TryParse(configuration["AppSettings:PreviousDayUtcDataLoadDelayHours:Value"], out int previousDayUtcDataLoadDelayHours))
        {
            throw new ConfigurationValidationException("PreviousDayUtcDataLoadDelayHours:Value must be an integer");
        }

        ValidatePreviousDayUtcDataLoadDelayHoursValue(previousDayUtcDataLoadDelayHours);
    }

    public static void ValidatePreviousDayUtcDataLoadDelayHoursValue(int previousDayUtcDataLoadDelayHour)
    {
        if (previousDayUtcDataLoadDelayHour < 0 || previousDayUtcDataLoadDelayHour > 23)
        {
            throw new ConfigurationValidationException($"PreviousDayUtcDataLoadDelayHours:Value must be between 0 and 23 inclusive, got {previousDayUtcDataLoadDelayHour}");
        }
    }

    public static void UpdatePreviousDayUtcDataLoadDelayHours(int newPreviousDayUtcDataLoadDelayHours)
    {
        ValidatePreviousDayUtcDataLoadDelayHoursValue(newPreviousDayUtcDataLoadDelayHours);

        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        if (!File.Exists(configFilePath))
        {
            throw new ConfigurationValidationException($"Configuration file not found: {configFilePath}");
        }

        try
        {
            string json = File.ReadAllText(configFilePath);

            using var document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;

            var jsonObject = JsonSerializer.Deserialize<JsonNode>(json);

            if (jsonObject!["AppSettings"] == null)
            {
                jsonObject["AppSettings"] = new JsonObject();
            }

            if (jsonObject["AppSettings"]!["PreviousDayUtcDataLoadDelayHours"] == null)
            {
                jsonObject!["AppSettings"]!["PreviousDayUtcDataLoadDelayHours"] = new JsonObject();
            }

            jsonObject["AppSettings"]!["PreviousDayUtcDataLoadDelayHours"]!["Value"] = newPreviousDayUtcDataLoadDelayHours;

            // Write updated JSON back to file with indentation
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string updatedJson = JsonSerializer.Serialize(jsonObject, options);
            File.WriteAllText(configFilePath, updatedJson);
        }
        catch (Exception ex) when (ex is not ConfigurationValidationException)
        {
            throw new ConfigurationValidationException($"Failed to update configuration file: {ex.Message}");
        }
    }
}