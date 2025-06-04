using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class MonthComparisonCalculator
{
    public static MonthComparisonResult CalculateComparison(
        List<DailyCostData> currentMonthData,
        List<DailyCostData> previousMonthData)
    {
        var currentDayCount = currentMonthData.Count;
        var previousMonthTotalDays = previousMonthData.Count;

        // Calculate the maximum days we can compare like-for-like
        var likeForLikeDays = Math.Min(currentDayCount, previousMonthTotalDays);

        // Like-for-like comparison (same number of days)
        var currentLikeForLike = currentMonthData.Take(likeForLikeDays).Average(dc => dc.Cost);
        var previousLikeForLike = previousMonthData.Take(likeForLikeDays).Average(dc => dc.Cost);

        // Full current month vs full previous month
        var currentFullAverage = currentMonthData.Average(dc => dc.Cost);
        var previousFullAverage = previousMonthData.Average(dc => dc.Cost);

        return new MonthComparisonResult
        {
            // Like-for-like comparison
            LikeForLikeDays = likeForLikeDays,
            CurrentLikeForLikeAverage = currentLikeForLike,
            PreviousLikeForLikeAverage = previousLikeForLike,
            LikeForLikeDelta = currentLikeForLike - previousLikeForLike,

            // Full month comparison
            CurrentDayCount = currentDayCount,
            PreviousMonthTotalDays = previousMonthTotalDays,
            CurrentFullAverage = currentFullAverage,
            PreviousFullAverage = previousFullAverage,
            FullComparisonDelta = currentFullAverage - previousFullAverage,

            // Indicates if we're comparing different day counts
            IsUnevenComparison = currentDayCount != previousMonthTotalDays
        };
    }
}