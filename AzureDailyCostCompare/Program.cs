using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureDailyCostCompare;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = BuildCommandLine();
        return await rootCommand.InvokeAsync(args);
    }

    private static RootCommand BuildCommandLine()
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
            await RunCostComparisonAsync(date, showWeeklyPatterns, showDayOfWeekAverages),
            dateOption, weeklyPatternOption, dayOfWeekOption);

        return rootCommand;
    }

    private static async Task RunCostComparisonAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages)
    {
        try
        {
            var accessToken = await AuthenticationService.GetAccessToken();
            var billingAccountId = await BillingService.GetBillingAccountIdAsync(accessToken);
            var dateHelper = date.HasValue ? new DateHelperService(date.Value) : new DateHelperService();

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