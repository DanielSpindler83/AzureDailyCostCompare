namespace AzureDailyCostCompare;

class Program
{
    static async Task Main()
    {
        try
        {
            var accessToken = await AuthenticationService.GetAccessToken();
            var billingAccountId = await BillingService.GetBillingAccountIdAsync(await AuthenticationService.GetAccessToken());

            // we can now add optional app command line parameter for historical date and pass into dateHelperService
            // DateTime lastDayOfNovember = new DateTime(2024, 11, 30);\
            // var dateHelperService = new DateHelperService(lastDayOfNovember);

            var dateHelperService = new DateHelperService();

            var costService = new CostService();

            var costData = await costService.QueryCostManagementAPI(accessToken, billingAccountId, dateHelperService.FirstDayOfPreviousMonth, dateHelperService.DataReferenceDate);

            var reportGenerator = new ReportGenerator(costData, dateHelperService);
            reportGenerator.GenerateDailyCostReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}