using AzureDailyCostCompare.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AzureDailyCostCompare.Application;

public class ApplicationUnifiedSettings(
    PreviousDayUtcDataLoadDelayHoursUserSetting previousDayUtcDataLoadDelayHoursUserSetting
    )
{
    private readonly PreviousDayUtcDataLoadDelayHoursUserSetting _previousDayUtcDataLoadDelayUserSetting = previousDayUtcDataLoadDelayHoursUserSetting;

    public const string ConfigFileName = "appsettings.json";

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool ShowWeeklyPatterns { get; set; } = false;
    public bool ShowDayOfWeekAverages { get; set; } = false;
    public int? PreviousDayUtcDataLoadDelayHoursCommandLine { get; set; }
    public int PreviousDayUtcDataLoadDelayHours { get; private set; } = 4; // default value of 4 - but should always be set via file config or command line

    public void DeterminePreviousDayUtcDataLoadDelayHours()
    {
        if (!PreviousDayUtcDataLoadDelayHoursCommandLine.HasValue) // if NO commandline value we use user settings
        {
            PreviousDayUtcDataLoadDelayHours = _previousDayUtcDataLoadDelayUserSetting.NumberOfHours ?? 4;
        }

        if (PreviousDayUtcDataLoadDelayHoursCommandLine.HasValue)
        {
            // commandline passed in value for PreviousDayUtcDataLoadDelayHours and we should use it and store it in user settings(app settings for now)
            //QUESTION - has command line value been validated? I dont think so...
            ValidatePreviousDayUtcDataLoadDelayHoursValue(PreviousDayUtcDataLoadDelayHoursCommandLine);
            PreviousDayUtcDataLoadDelayHours = PreviousDayUtcDataLoadDelayHoursCommandLine ?? 4;
            LoadConfiguration();
            UpdatePreviousDayUtcDataLoadDelayHours(PreviousDayUtcDataLoadDelayHours);

            DisplaySuccess($"PreviousDayUtcDataLoadDelayHoursUserSetting value successfully updated to {PreviousDayUtcDataLoadDelayHours} " +
              $"in {ConfigFileName}. New value will be used now and for subsequent executions.");

        }
    }

    public static void ValidatePreviousDayUtcDataLoadDelayHoursValue(int? previousDayUtcDataLoadDelayHours)
    {
        if (previousDayUtcDataLoadDelayHours < 0 || previousDayUtcDataLoadDelayHours > 23)
        {
            throw new ConfigurationValidationException($"PreviousDayUtcDataLoadDelayHoursUserSetting:Value must be between 0 and 23 inclusive, got {previousDayUtcDataLoadDelayHours}");
        }
    }

    public static IConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(ConfigFileName, optional: false, reloadOnChange: false);

        return builder.Build();
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
            var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

            // Update the value while preserving the description
            config.PreviousDayUtcDataLoadDelayHoursUserSetting.NumberOfHours = newPreviousDayUtcDataLoadDelayHours;

            // If description is empty (new config), set the default description
            if (string.IsNullOrEmpty(config.PreviousDayUtcDataLoadDelayHoursUserSetting.Description))
            {
                config.PreviousDayUtcDataLoadDelayHoursUserSetting.Description =
                    "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23. Value not used when working with historical dates from past months.";
            }

            // Serialize back to file
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string updatedJson = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configFilePath, updatedJson);
        }
        catch (Exception ex) when (ex is not ConfigurationValidationException)
        {
            throw new ConfigurationValidationException($"Failed to update configuration file: {ex.Message}");
        }
    }

    public class PreviousDayUtcDataLoadDelayHoursConfig
    {
        public int NumberOfHours { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // wrapper class for the root JSON
    public class AppConfig
    {
        public PreviousDayUtcDataLoadDelayHoursConfig PreviousDayUtcDataLoadDelayHoursUserSetting { get; set; } = new();
    }

    private static void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
