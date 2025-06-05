using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class CostComparisonBusinessHandler(
    AuthenticationService authService,
    BillingService billingService,
    CostService costService,
    ApplicationUnifiedSettings applicationUnifiedSettings,
    CostComparisonDateService costComparisonDateService,
    ReportGenerator reportGenerator)
{
    private readonly AuthenticationService _authService = authService;
    private readonly BillingService _billingService = billingService;
    private readonly CostService _costService = costService;
    private readonly ApplicationUnifiedSettings _applicationUnifiedSettings = applicationUnifiedSettings;
    private readonly CostComparisonDateService _costComparisonDateService = costComparisonDateService;
    private readonly ReportGenerator _reportGenerator = reportGenerator;

    public async Task RunAsync()
    {
        try
        {
            await RunCostComparisonAsync(_applicationUnifiedSettings.Date, 
                                         _applicationUnifiedSettings.PreviousDayUtcDataLoadDelayHours);
        }
        catch (ConfigurationValidationException ex)
        {
            DisplayError(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
            throw;
        }
    }

    private async Task RunCostComparisonAsync(
        DateTime date,
        int previousDayUtcDataLoadDelayHours)
    {
        var accessToken = await _authService.GetAccessToken();

        var billingAccountId = await _billingService.GetBillingAccountIdAsync(accessToken);

        //var costComparisonContext = date.HasValue
        //    ? _costComparisonDateService.CreateContextWithOverride(previousDayUtcDataLoadDelayHours, date.Value)
        //    : _costComparisonDateService.CreateContext(previousDayUtcDataLoadDelayHours);

        var costComparisonContext = _costComparisonDateService.CreateContext(date, previousDayUtcDataLoadDelayHours);

        var costData = await _costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            costComparisonContext.PreviousMonthStart,// start date
            costComparisonContext.ReferenceDate); // end date

        _reportGenerator.GenerateDailyCostReport(costData, costComparisonContext, _applicationUnifiedSettings.ShowWeeklyPatterns, _applicationUnifiedSettings.ShowDayOfWeekAverages);
    }

    private static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

}