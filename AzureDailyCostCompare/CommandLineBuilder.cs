using Microsoft.Extensions.Configuration;
using System.CommandLine;

namespace AzureDailyCostCompare;

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

        rootCommand.SetHandler(
            async context => await ExecuteCommandAsync(
                context.ParseResult.GetValueForOption(dateOption),
                context.ParseResult.GetValueForOption(weeklyPatternOption),
                context.ParseResult.GetValueForOption(dayOfWeekOption),
                context.ParseResult.GetValueForOption(dataLoadDelayOption)
            )
        );

        return rootCommand;
    }

    private static (
        Option<DateTime?> DateOption,
        Option<bool> WeeklyPatternOption,
        Option<bool> DayOfWeekOption,
        Option<int?> DataLoadDelayOption
    ) CreateCommandOptions()
    {
        return (
            new Option<DateTime?>(
                "--date",
                "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used."),

            new Option<bool>(
                aliases: ["--weekly-pattern-analysis", "-wpa"],
                description: "Show weekly pattern analysis comparing corresponding weekdays"),

            new Option<bool>(
                aliases: ["--day-of-week-averages", "-dowa"],
                description: "Show day of week averages comparing cost trends by weekday"),

            new Option<int?>(
                aliases: ["--previous-day-utc-data-load-delay", "-pdl"],
                description: "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23.")
        );
    }

    private static async Task ExecuteCommandAsync(
        DateTime? date,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages,
        int? previousDayUtcDataLoadDelayHours)
    {
        try
        {
            int dataLoadDelayHours = ConfigureDataLoadDelayHours(previousDayUtcDataLoadDelayHours);
            await RunCostComparisonAsync(date, showWeeklyPatterns, showDayOfWeekAverages, dataLoadDelayHours);
        }
        catch (ConfigurationValidationException ex)
        {
            DisplayError(ex.Message);
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
        }
    }

    private static int ConfigureDataLoadDelayHours(int? commandLineValue)
    {
        if (!commandLineValue.HasValue)
        {
            // Load from configuration
            IConfiguration configuration = ConfigurationManager.LoadConfiguration();
            ConfigurationManager.ValidatePreviousDayUtcDataLoadDelayHours(configuration);
            return configuration.GetValue<int>("AppSettings:PreviousDayUtcDataLoadDelayHours:Value");
        }

        // Use and save command line value
        ConfigurationManager.ValidatePreviousDayUtcDataLoadDelayHoursValue(commandLineValue.Value);
        ConfigurationManager.UpdatePreviousDayUtcDataLoadDelayHours(commandLineValue.Value);

        DisplaySuccess($"PreviousDayUtcDataLoadDelayHours value successfully updated to {commandLineValue.Value} " +
                      $"in {ConfigurationManager.ConfigFileName}. New value will be used now and for subsequent executions.");

        return commandLineValue.Value;
    }

    private static async Task RunCostComparisonAsync(
        DateTime? date,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages,
        int previousDayUtcDataLoadDelayHours)
    {
        // Get authentication token
        var accessToken = await AuthenticationService.GetAccessToken();

        // Get billing account ID
        var billingAccountId = await BillingService.GetBillingAccountIdAsync(accessToken);

        // Create date helper (with optional reference date)
        var dateHelper = date.HasValue
            ? new DateHelperService(previousDayUtcDataLoadDelayHours, date.Value)
            : new DateHelperService(previousDayUtcDataLoadDelayHours);

        // Query cost data
        var costService = new CostService();
        var costData = await costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            dateHelper.FirstDayOfPreviousMonth,
            dateHelper.DataReferenceDate);

        // Generate report
        new ReportGenerator(costData, dateHelper)
            .GenerateDailyCostReport(showWeeklyPatterns, showDayOfWeekAverages);
    }

    private static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

    private static void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}