using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class ApplicationUnifiedSettings
{
    private readonly UserSettings _userSettings;

    public ApplicationUnifiedSettings(UserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public DateTime InputComparisonDate { get; private set; } = DateTime.UtcNow;
    public bool ShowWeeklyPatterns { get; private set; } = false;
    public bool ShowDayOfWeekAverages { get; private set; } = false;
    public int PreviousDayUtcDataLoadDelayHours { get; private set; } = 4; // default value of 4 - but should always be set via file config or command line

    public void SetApplicationUnifiedSettings(DateTime? comparisonReferenceDateFromCommandLine, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHoursFromCommandLine)
    {
        if (comparisonReferenceDateFromCommandLine is not null) // if a date was passed in via command line we use it
        {
            InputComparisonDate = (DateTime)comparisonReferenceDateFromCommandLine;
        }

        if (comparisonReferenceDateFromCommandLine is null) // if no date passed in we use the current UTC date time
        {
            InputComparisonDate = DateTime.UtcNow;
        }

        ShowWeeklyPatterns = showWeeklyPatterns;
        ShowDayOfWeekAverages = showDayOfWeekAverages;

        if (previousDayUtcDataLoadDelayHoursFromCommandLine is null) // if NO commandline value we use user settings
        {
            PreviousDayUtcDataLoadDelayHours = _userSettings.PreviousDayUtcDataLoadDelayHours.NumberOfHours ?? 4;
        }

        if (previousDayUtcDataLoadDelayHoursFromCommandLine is not null) 
        {
            ValidatePreviousDayUtcDataLoadDelayHoursValue(previousDayUtcDataLoadDelayHoursFromCommandLine);
            PreviousDayUtcDataLoadDelayHours = previousDayUtcDataLoadDelayHoursFromCommandLine ?? 4;

            UserSettings.SaveSetting(
                "PreviousDayUtcDataLoadDelayHoursUserSetting",
                new { NumberOfHours = PreviousDayUtcDataLoadDelayHours });
        }
    }

    public static void ValidatePreviousDayUtcDataLoadDelayHoursValue(int? previousDayUtcDataLoadDelayHours)
    {
        if (previousDayUtcDataLoadDelayHours < 0 || previousDayUtcDataLoadDelayHours > 23)
        {
            throw new ConfigurationValidationException($"PreviousDayUtcDataLoadDelayHoursUserSetting:Value must be between 0 and 23 inclusive, got {previousDayUtcDataLoadDelayHours}");
        }
    }
}
