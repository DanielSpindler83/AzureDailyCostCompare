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
            var startDate = DateTime.UtcNow.AddMonths(-1).Date;
            var endDate = DateTime.UtcNow.Date;
            var costData = await costService.QueryCostManagementAPI(accessToken, billingAccountId, startDate, endDate);

            var reportGenerator = new ReportGenerator();
            reportGenerator.GenerateDailyCostReport(costData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}