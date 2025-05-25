// Business logic interfaces and implementations

namespace AzureDailyCostCompare.Application.Interfacces;

public interface ICostComparisonHandler
{
    Task RunAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHours);
}
