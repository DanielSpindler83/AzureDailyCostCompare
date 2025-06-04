using AzureDailyCostCompare.Application;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AzureDailyCostCompare.Infrastructure;

public static class CommandLineBuilder
{
    public static RootCommand BuildCommandLine(ServiceProvider serviceProvider)
    {
        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool")
        {
            Name = "azure-daily-cost-compare" 
        };

        var (dateOption, showWeeklyPatternsOption, showDayOfWeekAveragesOption, previousDayUtcDataLoadDelayHoursOption) = CreateCommandOptions();

        rootCommand.AddOption(dateOption);
        rootCommand.AddOption(showWeeklyPatternsOption);
        rootCommand.AddOption(showDayOfWeekAveragesOption);
        rootCommand.AddOption(previousDayUtcDataLoadDelayHoursOption);

        // Command execution handler
        rootCommand.SetHandler(async (date, showWeeklyPatterns, showDayOfWeekAverages, previousDayUtcDataLoadDelayHours) =>
        {
            try
            {
                // Get the singleton applicationUnifiedSettingsService instance and populate it
                var applicationUnifiedSettingsService = serviceProvider.GetRequiredService<ApplicationUnifiedSettings>();

                applicationUnifiedSettingsService.Date = date; // refactor oppotunity - we set date here if its null: applicationUnifiedSettingsService.Date = date ?? DateTime.UtcNow;
                // we then eliminate the need for override date and normal date logic seperation in CostComparisonDateService as we just validate it via same logic path from here on

                applicationUnifiedSettingsService.ShowWeeklyPatterns = showWeeklyPatterns;
                applicationUnifiedSettingsService.ShowDayOfWeekAverages = showDayOfWeekAverages;
                applicationUnifiedSettingsService.PreviousDayUtcDataLoadDelayHoursCommandLine = previousDayUtcDataLoadDelayHours;

                // Now delegate to your actual application logic
                var app = serviceProvider.GetRequiredService<CostComparisonBusinessHandler>();
                await app.RunAsync(); //Auto injects the ApplicationUnifiedSettings to get access to above ApplicationUnifiedSettings values
            }
            catch (Exception ex) { Console.WriteLine($"Service1 failed: {ex}"); }

        }, dateOption, showWeeklyPatternsOption, showDayOfWeekAveragesOption, previousDayUtcDataLoadDelayHoursOption);

        return rootCommand;
    }

    private static (
        Option<DateTime?> DateOption,
        Option<bool> ShowWeeklyPatternsOptio,
        Option<bool> ShowDayOfWeekAveragesOption,
        Option<int?> PreviousDayUtcDataLoadDelayHoursOption
    ) CreateCommandOptions()
    {
        var dateOption = new Option<DateTime?>(
            "--date",
            "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.")
        {
            ArgumentHelpName = "yyyy-MM-dd"
        };

        var showWeeklyPatternsOption = new Option<bool>(
            aliases: ["--weekly-pattern-analysis", "-wpa"],
            description: "Show weekly pattern analysis comparing corresponding weekdays");

        var showDayOfWeekAveragesOption = new Option<bool>(
            aliases: ["--day-of-week-averages", "-dowa"],
            description: "Show day of week averages comparing cost trends by weekday");

        var previousDayUtcDataLoadDelayHoursOption = new Option<int?>(
            aliases: ["--previous-day-utc-data-load-delay", "-pdl"],
            description: "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23. Setting is used now and persists for future executions.")
        {
            ArgumentHelpName = "int:0-23"
        };

        return (dateOption, showWeeklyPatternsOption, showDayOfWeekAveragesOption, previousDayUtcDataLoadDelayHoursOption);
    }
}