namespace AzureDailyCostCompare;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var authenticationService = new AuthenticationService();
            var accessToken = await authenticationService.GetAccessToken();

            var billingService = new BillingService();
            var billingAccountId = await billingService.GetBillingAccountIdAsync(accessToken);

            var dateHelperService = new DateHelperService();

            var costService = new CostService();

            var costData = await costService.QueryCostManagementAPI(accessToken, billingAccountId, dateHelperService.FirstDayOfPreviousMonth, dateHelperService.Today);

            var reportGenerator = new ReportGenerator(costData, dateHelperService);
            reportGenerator.GenerateDailyCostReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}