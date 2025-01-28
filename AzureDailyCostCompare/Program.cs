using System.CommandLine;
using System.CommandLine.Parsing;

namespace AzureDailyCostCompare;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var dateOption = new Option<DateTime?>(
            name: "--date",
            description: "Optional reference date for the report (format: yyyy-MM-dd). If not provided, current date will be used.");

        var rootCommand = new RootCommand("Azure Daily Cost Comparison Tool");
        rootCommand.AddOption(dateOption);

        rootCommand.SetHandler(async (date) =>
        {
            try
            {
                var accessToken = await AuthenticationService.GetAccessToken();
                var billingAccountId = await BillingService.GetBillingAccountIdAsync(await AuthenticationService.GetAccessToken());

                var dateHelperService = date.HasValue
                    ? new DateHelperService(date.Value)
                    : new DateHelperService();

                var costService = new CostService();
                var costData = await costService.QueryCostManagementAPI(
                    accessToken,
                    billingAccountId,
                    dateHelperService.FirstDayOfPreviousMonth,
                    dateHelperService.DataReferenceDate);

                var reportGenerator = new ReportGenerator(costData, dateHelperService);
                reportGenerator.GenerateDailyCostReport();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }, dateOption);

        return await rootCommand.InvokeAsync(args);
    }
}