using System.CommandLine;

namespace AzureDailyCostCompare.Infrastructure;

public static class CommandLineBuilder
{
    public static RootCommand BuildCommandLine()
    {
        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool")
        {
            Name = "azure-daily-cost-compare"
        };

        var (dateOption, weeklyPatternOption, dayOfWeekOption, dataLoadDelayOption) = CreateCommandOptions();

        rootCommand.AddOption(dateOption);
        rootCommand.AddOption(weeklyPatternOption);
        rootCommand.AddOption(dayOfWeekOption);
        rootCommand.AddOption(dataLoadDelayOption);

        // Register handler - System.CommandLine will execute this, not this builder
        rootCommand.SetHandler(CostComparisonHandler.ExecuteAsync,
            dateOption, weeklyPatternOption, dayOfWeekOption, dataLoadDelayOption);

        return rootCommand;
    }

    private static (
        Option<DateTime?> DateOption,
        Option<bool> WeeklyPatternOption,
        Option<bool> DayOfWeekOption,
        Option<int?> DataLoadDelayOption
    ) CreateCommandOptions()
    {
        var dateOption = new Option<DateTime?>(
            "--date",
            "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.")
        {
            ArgumentHelpName = "yyyy-MM-dd"
        };

        var weeklyPatternOption = new Option<bool>(
            aliases: ["--weekly-pattern-analysis", "-wpa"],
            description: "Show weekly pattern analysis comparing corresponding weekdays");

        var dayOfWeekOption = new Option<bool>(
            aliases: ["--day-of-week-averages", "-dowa"],
            description: "Show day of week averages comparing cost trends by weekday");

        var dataLoadDelayOption = new Option<int?>(
            aliases: ["--previous-day-utc-data-load-delay", "-pdl"],
            description: "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23. Setting is used now and persists for future executions.")
        {
            ArgumentHelpName = "int:0-23"
        };

        return (dateOption, weeklyPatternOption, dayOfWeekOption, dataLoadDelayOption);
    }
}