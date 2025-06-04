using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class CostDataProcessor
{
    public static ProcessedCostData ProcessCostData(List<DailyCostData> costData, CostComparisonContext context)
    {
        var currentMonthData = costData
            .Where(dc => dc.DateString.Month == context.CurrentMonthStart.Month &&
                        dc.DateString.Year == context.CurrentMonthStart.Year)
            .ToList();

        var previousMonthData = costData
            .Where(dc => dc.DateString.Month == context.PreviousMonthStart.Month &&
                        dc.DateString.Year == context.PreviousMonthStart.Year)
            .ToList();

        var monthComparison = MonthComparisonCalculator.CalculateComparison(currentMonthData, previousMonthData);

        return new ProcessedCostData
        {
            CurrentMonthCostData = currentMonthData,
            PreviousMonthCostData = previousMonthData,
            MonthComparison = monthComparison
        };
    }
}
