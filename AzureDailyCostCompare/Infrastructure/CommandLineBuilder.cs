using AzureDailyCostCompare.Application;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AzureDailyCostCompare.Infrastructure;

public class CommandLineBuilder
{
    private readonly ApplicationUnifiedSettings _applicationUnifiedSettings;
    private readonly CostComparisonBusinessHandler _costComparisonBusinessHandler;

    public CommandLineBuilder(ApplicationUnifiedSettings applicationUnifiedSettings, CostComparisonBusinessHandler costComparisonBusinessHandler)
    {
        _applicationUnifiedSettings = applicationUnifiedSettings;
        _costComparisonBusinessHandler = costComparisonBusinessHandler;
    }

    public RootCommand BuildCommandLine()
    {
        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool")
        {
            Name = "azure-daily-cost-compare"
        };

        var options = CreateCommandOptions();

        rootCommand.AddOption(options.DateOption);
        rootCommand.AddOption(options.WeeklyPatternsOption);
        rootCommand.AddOption(options.DayOfWeekAveragesOption);
        rootCommand.AddOption(options.DataLoadDelayOption);

        // the binding of these is from the order of the options and we can name them arbitrarily here based on position\order of the options declared
        rootCommand.SetHandler(async (comparisonReferenceDateFromCommandLine, showWeeklyPatterns, showDayOfWeekAverages, previousDayUtcDataLoadDelayHoursFromCommandLine) =>
        {
            try
            {
                // setup our main application settings
                _applicationUnifiedSettings.SetApplicationUnifiedSettings(comparisonReferenceDateFromCommandLine, showWeeklyPatterns, showDayOfWeekAverages, previousDayUtcDataLoadDelayHoursFromCommandLine);

                // kick off the main business logic handler
                await _costComparisonBusinessHandler.RunAsync();
            }
            catch (Exception ex) { Console.WriteLine($"Service1 failed: {ex}"); }

        }, options.DateOption, options.WeeklyPatternsOption, options.DayOfWeekAveragesOption, options.DataLoadDelayOption);

        return rootCommand;
    }

    public record CommandLineOptions(
        Option<DateTime?> DateOption,
        Option<bool> WeeklyPatternsOption,
        Option<bool> DayOfWeekAveragesOption,
        Option<int?> DataLoadDelayOption
    );

    private static CommandLineOptions CreateCommandOptions()
    {
        var dateOption = new Option<DateTime?>(
            "--date",
            "Optional comparison reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.")
        {
            ArgumentHelpName = "yyyy-MM-dd"
        };

        var weeklyPatternsOption = new Option<bool>(
            aliases: ["--weekly-pattern-analysis", "-wpa"],
            description: "Show weekly pattern analysis comparing corresponding weekdays");

        var dayOfWeekAveragesOption = new Option<bool>(
            aliases: ["--day-of-week-averages", "-dowa"],
            description: "Show day of week averages comparing cost trends by weekday");

        var dataLoadDelayOption = new Option<int?>(
            aliases: ["--previous-day-utc-data-load-delay", "-pdl"],
            description: "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23. Setting is used now and persists for future executions.")
        {
            ArgumentHelpName = "int:0-23"
        };

        return new CommandLineOptions(dateOption, weeklyPatternsOption, dayOfWeekAveragesOption, dataLoadDelayOption);
    }
}