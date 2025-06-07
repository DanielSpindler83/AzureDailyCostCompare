using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class CostDataProcessor
{
    public ProcessedCostData ProcessCostData(List<DailyCostData> costData, CostComparisonContext context)
    {
        // Filter and sort data for each month
        var currentMonthDailyCosts = costData
            .Where(dc => dc.DateString.Month == context.MonthComparisonPeriod.CurrentFirstDayOfMonth.Month &&
                   dc.DateString.Year == context.MonthComparisonPeriod.CurrentFirstDayOfMonth.Year)
            .OrderBy(dc => dc.DateString)
            .ToList();

        var previousMonthDailyCosts = costData
            .Where(dc => dc.DateString.Month == context.MonthComparisonPeriod.PreviousFirstDayOfMonth.Month &&
                   dc.DateString.Year == context.MonthComparisonPeriod.PreviousFirstDayOfMonth.Year)
            .OrderBy(dc => dc.DateString)
            .ToList();

        // Calculate day counts
        var currentMonthDayCount = currentMonthDailyCosts.Count;
        var previousMonthDayCount = previousMonthDailyCosts.Count; // Always full month
        var likeForLikeDayCount = Math.Min(currentMonthDayCount, previousMonthDayCount);

        // Calculate averages for like-for-like comparison (same number of days)
        var currentMonthLikeForLikeAverage = currentMonthDailyCosts
            .Take(likeForLikeDayCount)
            .Average(dc => dc.Cost);

        var previousMonthLikeForLikeAverage = previousMonthDailyCosts
            .Take(likeForLikeDayCount)
            .Average(dc => dc.Cost);

        // Always calculate full previous month average (complete month data)
        var previousMonthFullAverage = previousMonthDailyCosts.Average(dc => dc.Cost);

        // Calculate extra days average ONLY if current month has more days than previous
        decimal? currentMonthExtraDaysAverage = null;
        if (currentMonthDayCount > previousMonthDayCount)
        {
            currentMonthExtraDaysAverage = currentMonthDailyCosts
                .Skip(previousMonthDayCount) // Skip the days already compared
                .Average(dc => dc.Cost);
        }

        return new ProcessedCostData
        {
            // Raw data
            CurrentMonthDailyCosts = currentMonthDailyCosts,
            PreviousMonthDailyCosts = previousMonthDailyCosts,

            // Day counts for transparency
            CurrentMonthDayCount = currentMonthDayCount,
            PreviousMonthDayCount = previousMonthDayCount,
            LikeForLikeDayCount = likeForLikeDayCount,

            // Like-for-like comparison (same number of days from each month)
            CurrentMonthLikeForLikeAverage = currentMonthLikeForLikeAverage,
            PreviousMonthLikeForLikeAverage = previousMonthLikeForLikeAverage,
            LikeForLikeDailyAverageDelta = currentMonthLikeForLikeAverage - previousMonthLikeForLikeAverage,

            // Full previous month context (always complete)
            PreviousMonthFullAverage = previousMonthFullAverage,

            // Extra days analysis (only when current month is longer)
            CurrentMonthExtraDaysAverage = currentMonthExtraDaysAverage
        };
    }
}