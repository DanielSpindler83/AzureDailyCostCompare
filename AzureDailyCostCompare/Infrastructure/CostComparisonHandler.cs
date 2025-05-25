using AzureDailyCostCompare.Application.Interfacces;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare.Infrastructure;

public static class CostComparisonHandler
{
    // Static method that System.CommandLine calls
    public static async Task ExecuteAsync(
        DateTime? date,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages,
        int? previousDayUtcDataLoadDelayHours)
    {
        if (ServiceProviderAccessor.ServiceProvider == null)
        {
            throw new InvalidOperationException("Service provider not initialized");
        }

        // Get the actual business handler from DI
        var handler = ServiceProviderAccessor.ServiceProvider.GetRequiredService<ICostComparisonHandler>();

        await handler.RunAsync(date, showWeeklyPatterns, showDayOfWeekAverages, previousDayUtcDataLoadDelayHours);
    }
}
