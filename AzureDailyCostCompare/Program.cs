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

            var costService = new CostService();

            var dateHelperService = new DateHelperService();

            var costData = await costService.QueryCostManagementAPI(accessToken, billingAccountId, dateHelperService.StartDate, dateHelperService.EndDate);

            var reportGenerator = new ReportGenerator();
            reportGenerator.GenerateDailyCostReport(costData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}