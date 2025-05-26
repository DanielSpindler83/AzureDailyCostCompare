namespace AzureDailyCostCompare.Application.Interfaces;

public interface ICostComparisonHandler
{
    Task RunAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHours);
}
