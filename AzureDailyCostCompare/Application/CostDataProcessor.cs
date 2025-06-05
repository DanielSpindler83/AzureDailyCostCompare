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

        var averageCurrentMonth = currentMonthData
            .Take(currentMonthData.Count)
            .Average(dc => dc.Cost);


        // For like-for-like comparison, use the same number of days from each month
        var daysToCompare = Math.Min(currentMonthData.Count, previousMonthData.Count);
        var averagePreviousMonthForSameDayCount = daysToCompare > 0 ?
            previousMonthData.Take(daysToCompare).Average(dc => dc.Cost) : 0;

        /*
        If current month = Jan (31 days)
            then previous month = Dec (30 days)
            our average for averagePreviousMonthForSameDayCount is skewed and incorrect as month is 30days but divided by 31.... only wrong on that last day? I think....

        If current month = Feb (28 days)
            then previous month = Jan (31 days)
            our average for averagePreviousMonthForSameDayCount is fine as we only ever calc up to 28 days - so kinda like for like to current month(but its not the full previous month(but we show that seperatley)

        so shit gets messy

        when current month is longer we need to change the output and show correct data
            maybe cap it to the lower of the two numbers for the running day comparison (only ever affects partial month - wchich is current not historical)
            BUT we also then need to show the current month average for the current number of days (i.e show the 31 days average for Jan)

        when current month is shorter i think we are ok as long as we show the full previous month average correctly as well the user sees both and its correct data

        */

        var averagePreviousFullMonth = previousMonthData.Average(dc => dc.Cost); //always a full month 

        return new ProcessedCostData
        {
            CurrentMonthCostData = currentMonthData,
            PreviousMonthCostData = previousMonthData,

            AverageCurrentPartialMonth = averageCurrentMonth,
            AveragePreviousPartialMonth = averagePreviousMonthForSameDayCount,

            AveragePreviousFullMonth = averagePreviousFullMonth,

            CurrentToPreviousMonthAveragesCostDelta = averageCurrentMonth - averagePreviousMonthForSameDayCount
        };
    }
}
