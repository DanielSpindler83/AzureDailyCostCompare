using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class CostComparisonBusinessHandler
{
    private readonly ApplicationUnifiedSettings _applicationUnifiedSettings;
    private readonly AuthenticationService _authService;
    private readonly BillingService _billingService;
    private readonly CostService _costService;
    private readonly CostComparisonDateService _costComparisonDateService;
    private readonly ReportGenerator _reportGenerator;

    public CostComparisonBusinessHandler(
        ApplicationUnifiedSettings applicationUnifiedSettings,
        AuthenticationService authService,
        BillingService billingService,
        CostService costService,
        CostComparisonDateService costComparisonDateService,
        ReportGenerator reportGenerator)
    {
        _applicationUnifiedSettings = applicationUnifiedSettings;
        _authService = authService;
        _billingService = billingService;
        _costService = costService;
        _costComparisonDateService = costComparisonDateService;
        _reportGenerator = reportGenerator;
    }

    public async Task RunAsync()
    {
        try
        {
            await RunCostComparisonAsync(_applicationUnifiedSettings.InputComparisonDate, 
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
        DateTime inputComparisonDate,
        int previousDayUtcDataLoadDelayHours)
    {
        var accessToken = await _authService.GetAccessToken();

        var billingAccountId = await _billingService.GetBillingAccountIdAsync(accessToken);

        var costComparisonContext = _costComparisonDateService.CreateContext(inputComparisonDate, previousDayUtcDataLoadDelayHours);

        var costData = await _costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            costComparisonContext.MonthComparisonPeriod.PreviousFirstDayOfMonth,
            costComparisonContext.ComparisonReferenceDate);

        _reportGenerator.GenerateDailyCostReport(costData, costComparisonContext, _applicationUnifiedSettings.ShowWeeklyPatterns, _applicationUnifiedSettings.ShowDayOfWeekAverages);
    }

    private static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

}