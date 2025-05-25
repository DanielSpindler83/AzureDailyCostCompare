using AzureDailyCostCompare.Application.Interfacces;
using AzureDailyCostCompare.Infrastructure;
using AzureDailyCostCompare.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = AzureDailyCostCompare.Infrastructure.ConfigurationManager;

namespace AzureDailyCostCompare.Application;

public class CostComparisonBusinessHandler(
    IAuthenticationService authService,
    IBillingService billingService,
    ICostService costService) : ICostComparisonHandler
{
    private readonly IAuthenticationService _authService = authService;
    private readonly IBillingService _billingService = billingService;
    private readonly ICostService _costService = costService;

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

        // Create date helper (with optional reference date)
        var dateHelper = date.HasValue
            ? new DateHelperService(previousDayUtcDataLoadDelayHours, date.Value)
            : new DateHelperService(previousDayUtcDataLoadDelayHours);

        // Query cost data
        var costData = await _costService.QueryCostManagementAPI(
            accessToken,
            billingAccountId,
            dateHelper.FirstDayOfPreviousMonth,
            dateHelper.DataReferenceDate);

        // Generate report
        new ReportGenerator(costData, dateHelper)
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