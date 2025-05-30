using AzureDailyCostCompare.Infrastructure;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = AzureDailyCostCompare.Infrastructure.ConfigurationManager;

namespace AzureDailyCostCompare.Application;

public class CostComparisonBusinessHandler(
    AuthenticationService authService,
    BillingService billingService,
    CostService costService)
{
    private readonly AuthenticationService _authService = authService;
    private readonly BillingService _billingService = billingService;
    private readonly CostService _costService = costService;

    public async Task RunAsync(
        DateTime? date,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages,
        int? previousDayUtcDataLoadDelayHours)
    {
        try
        {
            int dataLoadDelayHours = ConfigureDataLoadDelayHours(previousDayUtcDataLoadDelayHours);
            await RunCostComparisonAsync(date, showWeeklyPatterns, showDayOfWeekAverages, dataLoadDelayHours);
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

    private static int ConfigureDataLoadDelayHours(int? commandLineValue)
    {
        if (!commandLineValue.HasValue)
        {
            var configuration = ConfigurationManager.LoadConfiguration();
            ConfigurationManager.ValidatePreviousDayUtcDataLoadDelayHours(configuration);
            return configuration.GetValue<int>("AppSettings:PreviousDayUtcDataLoadDelayHours:Value");
        }

        ConfigurationManager.ValidatePreviousDayUtcDataLoadDelayHoursValue(commandLineValue.Value);
        ConfigurationManager.UpdatePreviousDayUtcDataLoadDelayHours(commandLineValue.Value);

        DisplaySuccess($"PreviousDayUtcDataLoadDelayHours value successfully updated to {commandLineValue.Value} " +
                      $"in {ConfigurationManager.ConfigFileName}. New value will be used now and for subsequent executions.");

        return commandLineValue.Value;
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
        var dateService = new CostComparisonDateService();
        var context = date.HasValue
            ? dateService.CreateContextWithOverride(previousDayUtcDataLoadDelayHours, date.Value)
            : dateService.CreateContext(previousDayUtcDataLoadDelayHours);

        // Query cost data (updated property names)
        var costData = await _costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            context.PreviousMonthStart,  // was dateHelper.PreviousMonthStartDate
            context.ReferenceDate);      // was dateHelper.CostDataReferenceDate

        // Generate report (pass context instead of dateHelper)
        new ReportGenerator(costData, context)
            .GenerateDailyCostReport(showWeeklyPatterns, showDayOfWeekAverages);
    }

    private static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

    private static void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}