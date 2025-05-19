using Microsoft.Extensions.Configuration;
using System.CommandLine;

namespace AzureDailyCostCompare;

public static class CommandLineBuilder
{
    public static ConfigurationService ConfigService { get; } = new ConfigurationService();

    public static RootCommand BuildCommandLine()
    {
        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool") { Name = "azure-daily-cost-compare" };

        var dateOption = new Option<DateTime?>(
            "--date",
            "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.");
        var weeklyPatternOption = new Option<bool>(
            ["--weekly-pattern-analysis", "-wpa"],
            "Show weekly pattern analysis comparing corresponding weekdays");
        var dayOfWeekOption = new Option<bool>(
            ["--day-of-week-averages", "-dowa"],
            "Show day of week averages comparing cost trends by weekday");

        var previousDayUtcDataLoadDelayHoursOption = new Option<int?>(
            ["--previous-day-utc-data-load-delay", "-pdl"],
            "Number of hours after midnight UTC used to determine when the previous day's Azure cost data is considered complete enough to load. For example, a value of 4 means data for the previous day is assumed complete at 04:00 UTC. Valid values: 0–23.");

        rootCommand.AddOption(dateOption);
        rootCommand.AddOption(weeklyPatternOption);
        rootCommand.AddOption(dayOfWeekOption);
        rootCommand.AddOption(previousDayUtcDataLoadDelayHoursOption);

        rootCommand.SetHandler(async (DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHoursCommandArg) =>
        {
            int previousDayUtcDataLoadDelayHours;

            try
            {
                if (!previousDayUtcDataLoadDelayHoursCommandArg.HasValue)
                {
                    IConfiguration configuration = ConfigurationService.LoadConfiguration();
                    ConfigurationService.ValidatePreviousDayUtcDataLoadDelayHours(configuration);
                    previousDayUtcDataLoadDelayHours = configuration.GetValue<int>("AppSettings:PreviousDayUtcDataLoadDelayHours:Value");

                }
                else
                {
                    ConfigurationService.ValidatePreviousDayUtcDataLoadDelayHoursValue(previousDayUtcDataLoadDelayHoursCommandArg.Value);
                    ConfigurationService.UpdatePreviousDayUtcDataLoadDelayHours(previousDayUtcDataLoadDelayHoursCommandArg.Value);

                    previousDayUtcDataLoadDelayHours = previousDayUtcDataLoadDelayHoursCommandArg.Value;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"PreviousDayUtcDataLoadDelayHours value successfully updated to {previousDayUtcDataLoadDelayHoursCommandArg.Value} in {ConfigurationService.ConfigFileName}. New value will be used now and for subsequent executions.");
                    Console.ResetColor();
                }
            }
            catch (ConfigurationValidationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                return;
            }

            // Then run the cost comparison if no validation errors
            await RunCostComparisonAsync(date, showWeeklyPatterns, showDayOfWeekAverages, previousDayUtcDataLoadDelayHours);
        }, dateOption, weeklyPatternOption, dayOfWeekOption, previousDayUtcDataLoadDelayHoursOption);

        return rootCommand;
    }

    private static async Task RunCostComparisonAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int previousDayUtcDataLoadDelayHours)
    {
        try
        {
            var accessToken = await AuthenticationService.GetAccessToken();
            var billingAccountId = await BillingService.GetBillingAccountIdAsync(accessToken);
            var dateHelper = date.HasValue ? new DateHelperService(previousDayUtcDataLoadDelayHours, date.Value) : new DateHelperService(previousDayUtcDataLoadDelayHours);
            var costService = new CostService();
            var costData = await costService.QueryCostManagementAPI(
                accessToken,
                billingAccountId,
                dateHelper.FirstDayOfPreviousMonth,
                dateHelper.DataReferenceDate);
            new ReportGenerator(costData, dateHelper).GenerateDailyCostReport(showWeeklyPatterns, showDayOfWeekAverages);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
    }
}