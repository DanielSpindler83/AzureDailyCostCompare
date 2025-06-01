using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class CostDataProcessor
{
    public ProcessedCostData ProcessCostData(List<DailyCostData> costData, CostComparisonContext context)
    {
        var currentMonthData = costData
            .Where(dc => dc.DateString.Month == context.CurrentMonthStart.Month &&
                        dc.DateString.Year == context.CurrentMonthStart.Year)
            .ToList();

        var previousMonthData = costData
            .Where(dc => dc.DateString.Month == context.PreviousMonthStart.Month &&
                        dc.DateString.Year == context.PreviousMonthStart.Year)
            .ToList();

        var averageCurrentPartialMonth = currentMonthData
            .Take(currentMonthData.Count)
            .Average(dc => dc.Cost);

        var averagePreviousPartialMonth = previousMonthData
            .Take(currentMonthData.Count)
            .Average(dc => dc.Cost);

        var averagePreviousFullMonth = previousMonthData.Average(dc => dc.Cost);

        return new ProcessedCostData
        {
            CurrentMonthCostData = currentMonthData,
            PreviousMonthCostData = previousMonthData,
            AverageCurrentPartialMonth = averageCurrentPartialMonth,
            AveragePreviousPartialMonth = averagePreviousPartialMonth,
            AveragePreviousFullMonth = averagePreviousFullMonth,
            CurrentToPreviousMonthAveragesCostDelta = averageCurrentPartialMonth - averagePreviousPartialMonth
        };
    }
}
