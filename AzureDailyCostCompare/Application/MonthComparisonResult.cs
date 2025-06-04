namespace AzureDailyCostCompare.Application;

public class MonthComparisonResult
{
    // Like-for-like comparison properties
    public int LikeForLikeDays { get; set; }
    public decimal CurrentLikeForLikeAverage { get; set; }
    public decimal PreviousLikeForLikeAverage { get; set; }
    public decimal LikeForLikeDelta { get; set; }

    // Full month comparison properties
    public int CurrentDayCount { get; set; }
    public int PreviousMonthTotalDays { get; set; }
    public decimal CurrentFullAverage { get; set; }
    public decimal PreviousFullAverage { get; set; }
    public decimal FullComparisonDelta { get; set; }

    // Metadata
    public bool IsUnevenComparison { get; set; }
}