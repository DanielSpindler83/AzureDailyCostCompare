namespace AzureDailyCostCompare;

class Program
{
    static async Task Main()
    {
        try
        {
            var accessToken = await AuthenticationService.GetAccessToken();

            var billingService = new BillingService();
            var billingAccountId = await billingService.GetBillingAccountIdAsync(accessToken);

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