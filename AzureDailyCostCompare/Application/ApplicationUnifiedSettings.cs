using AzureDailyCostCompare.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AzureDailyCostCompare.Application;

public class ApplicationUnifiedSettings
{
    public const string ConfigFileName = "appsettings.json";
    private readonly UserSettings _userSettings;

    public ApplicationUnifiedSettings(
        UserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public DateTime ComparisonReferenceDate { get; private set; } = DateTime.UtcNow;
    public bool ShowWeeklyPatterns { get; private set; } = false;
    public bool ShowDayOfWeekAverages { get; private set; } = false;
    public int PreviousDayUtcDataLoadDelayHours { get; private set; } = 4; // default value of 4 - but should always be set via file config or command line

    public void SetApplicationUnifiedSettings(DateTime? comparisonReferenceDateFromCommandLine, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHoursFromCommandLine)
    {
        if (comparisonReferenceDateFromCommandLine is not null) // if a date was passed in via command line we use it
        { 
            ComparisonReferenceDate = (DateTime)comparisonReferenceDateFromCommandLine; 
        }

        if (comparisonReferenceDateFromCommandLine is null) // if no date passed in we use the current UTC date time
        {
            ComparisonReferenceDate = DateTime.UtcNow;
        }

        ShowWeeklyPatterns = showWeeklyPatterns;
        ShowDayOfWeekAverages = showDayOfWeekAverages;

        if (previousDayUtcDataLoadDelayHoursFromCommandLine is null) // if NO commandline value we use user settings
        {
            PreviousDayUtcDataLoadDelayHours = _userSettings.PreviousDayUtcDataLoadDelayHours.NumberOfHours ?? 4;
        }

        if (previousDayUtcDataLoadDelayHoursFromCommandLine is not null) // commandline passed in value for PreviousDayUtcDataLoadDelayHours and we should use it and store it in user settings(app settings for now)
        {
            ValidatePreviousDayUtcDataLoadDelayHoursValue(previousDayUtcDataLoadDelayHoursFromCommandLine);
            PreviousDayUtcDataLoadDelayHours = previousDayUtcDataLoadDelayHoursFromCommandLine ?? 4;

            UserSettings.SaveSetting(
                "PreviousDayUtcDataLoadDelayHoursUserSetting",
                new { NumberOfHours = PreviousDayUtcDataLoadDelayHours });

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

    private static void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
