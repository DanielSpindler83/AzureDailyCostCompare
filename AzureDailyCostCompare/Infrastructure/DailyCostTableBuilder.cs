using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Infrastructure;

public class DailyCostTableBuilder
{
    public IEnumerable<DailyCostRow> BuildDailyCostTableData(ProcessedCostData data, CostComparisonContext context)
    {
        return from day in Enumerable.Range(1, context.ComparisonTableDayCount)
               let currentDay = FindDayInCurrentMonth(data.CurrentMonthCostData, day)
               let previousDay = FindDayInPreviousMonth(data.PreviousMonthCostData, day)
               select new DailyCostRow
               {
                   DayOfMonth = day,
                   PreviousCost = DetermineMonthCost(previousDay, day, context.PreviousMonthDayCount),
                   CurrentCost = DetermineMonthCost(currentDay, day, context.CurrentMonthDayCount),
                   CostDifference = CalculateCostDifference(currentDay, previousDay)
               };
    }

    private static DailyCostData? FindDayInCurrentMonth(List<DailyCostData> currentMonthData, int day) =>
        currentMonthData.FirstOrDefault(d => d.DateString.Day == day);

    private static DailyCostData? FindDayInPreviousMonth(List<DailyCostData> previousMonthData, int day) =>
        previousMonthData.FirstOrDefault(d => d.DateString.Day == day);

    private static decimal? DetermineMonthCost(DailyCostData? dayData, int day, int monthDaysCount) =>
        dayData?.Cost ?? (day > monthDaysCount ? null : 0);

    private static decimal CalculateCostDifference(DailyCostData? currentDay, DailyCostData? previousDay) =>
        (currentDay?.Cost ?? 0) - (previousDay?.Cost ?? 0);
}

public class DailyCostRow
{
    public int DayOfMonth { get; set; }
    public decimal? PreviousCost { get; set; }
    public decimal? CurrentCost { get; set; }
    public decimal CostDifference { get; set; }
}