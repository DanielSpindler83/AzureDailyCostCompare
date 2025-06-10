using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class ProcessedCostData
{
    // Raw data
    public List<DailyCostData> CurrentMonthDailyCosts { get; set; } = new();
    public List<DailyCostData> PreviousMonthDailyCosts { get; set; } = new();

    // Day counts (for transparency and validation)
    public int CurrentMonthDayCount { get; set; }
    public int PreviousMonthDayCount { get; set; }
    public int LikeForLikeDayCount { get; set; }

    // Like-for-like comparison (apples to apples)
    public decimal CurrentMonthLikeForLikeAverage { get; set; }
    public decimal PreviousMonthLikeForLikeAverage { get; set; }
    public decimal LikeForLikeDailyAverageDelta { get; set; }

    // Full previous month context (always complete month)
    public decimal PreviousMonthFullAverage { get; set; }

    // Extra days analysis (null if not applicable)
    public decimal? CurrentMonthExtraDaysAverage { get; set; }
}