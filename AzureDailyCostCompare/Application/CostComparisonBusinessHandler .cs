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
                                         _applicationUnifiedSettings.ShowWeeklyPatterns, 
                                         _applicationUnifiedSettings.ShowDayOfWeekAverages, 
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
        DateTime? date,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages,
        int previousDayUtcDataLoadDelayHours)
    {
        // Get authentication token
        var accessToken = await _authService.GetAccessToken();

        // Get billing account ID
        var billingAccountId = await _billingService.GetBillingAccountIdAsync(accessToken);

        // Create comparison context (replaces dateHelper creation)
        var context = date.HasValue
            ? _costComparisonDateService.CreateContextWithOverride(previousDayUtcDataLoadDelayHours, date.Value)
            : _costComparisonDateService.CreateContext(previousDayUtcDataLoadDelayHours);

        // Query cost data (updated property names)
        var costData = await _costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            context.PreviousMonthStart,  // was dateHelper.PreviousMonthStartDate
            context.ReferenceDate);      // was dateHelper.CostDataReferenceDate

        // Generate report
        _reportGenerator.GenerateDailyCostReport(costData, _applicationUnifiedSettings.ShowWeeklyPatterns, _applicationUnifiedSettings.ShowDayOfWeekAverages);
    }

    private static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

}