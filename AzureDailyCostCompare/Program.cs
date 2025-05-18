using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;

namespace AzureDailyCostCompare;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            IConfiguration configuration = LoadAndValidateConfiguration();

            // Cutoff hour setting has been validated
            int cutoffHourUtc = configuration.GetValue<int>("AppSettings:FullDayDataCutOffHourUTC:Value");

            var rootCommand = BuildCommandLine(cutoffHourUtc);
            return await rootCommand.InvokeAsync(args);
        }
        catch (ConfigurationValidationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Configuration error: {ex.Message}");
            Console.ResetColor();
            return 1; // Return non-zero exit code to indicate error
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    private static IConfiguration LoadAndValidateConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

        IConfiguration configuration = builder.Build();

        if (!configuration.GetSection("AppSettings:FullDayDataCutOffHourUTC:Value").Exists())
        {
            throw new ConfigurationValidationException("Missing required configuration value: AppSettings:FullDayDataCutOffHourUTC:Value");
        }

        if (!int.TryParse(configuration["AppSettings:FullDayDataCutOffHourUTC:Value"], out int cutoffHour))
        {
            throw new ConfigurationValidationException("FullDayDataCutOffHourUTC:Value must be an integer");
        }

        if (cutoffHour < 0 || cutoffHour > 23)
        {
            throw new ConfigurationValidationException($"FullDayDataCutOffHourUTC:Value must be between 0 and 23 inclusive, got {cutoffHour}");
        }

        return configuration;
    }

    private static RootCommand BuildCommandLine(int cutoffHourUtc)
    {
        var dateOption = new Option<DateTime?>(
            "--date",
            "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.");
        var weeklyPatternOption = new Option<bool>(
            ["--weekly-pattern-analysis", "-wpa"],
            "Show Weekly Pattern Analysis comparing corresponding weekdays");
        var dayOfWeekOption = new Option<bool>(
            ["--day-of-week-averages", "-dowa"],
            "Show Day of Week Averages comparing cost trends by weekday");
        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool") { Name = "azure-daily-cost-compare" };
        rootCommand.AddOption(dateOption);
        rootCommand.AddOption(weeklyPatternOption);
        rootCommand.AddOption(dayOfWeekOption);
        rootCommand.SetHandler(async (DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages) =>
            await RunCostComparisonAsync(date, showWeeklyPatterns, showDayOfWeekAverages, cutoffHourUtc),
            dateOption, weeklyPatternOption, dayOfWeekOption);
        return rootCommand;
    }

    private static async Task RunCostComparisonAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int cutoffHourUtc)
    {
        try
        {
            var accessToken = await AuthenticationService.GetAccessToken();
            var billingAccountId = await BillingService.GetBillingAccountIdAsync(accessToken);
            var dateHelper = date.HasValue ? new DateHelperService(cutoffHourUtc, date.Value) : new DateHelperService(cutoffHourUtc);
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
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

public class ConfigurationValidationException(string message) : Exception(message)
{
}
