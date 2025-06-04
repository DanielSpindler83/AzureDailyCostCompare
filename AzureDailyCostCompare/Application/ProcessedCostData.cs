using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class ProcessedCostData
{
    public List<DailyCostData> CurrentMonthCostData { get; set; } = [];
    public List<DailyCostData> PreviousMonthCostData { get; set; } = [];

    // Legacy properties (keep for backward compatibility if needed)
    public decimal AverageCurrentPartialMonth => MonthComparison?.CurrentFullAverage ?? 0;
    public decimal AveragePreviousPartialMonth => MonthComparison?.PreviousLikeForLikeAverage ?? 0;
    public decimal AveragePreviousFullMonth => MonthComparison?.PreviousFullAverage ?? 0;
    public decimal CurrentToPreviousMonthAveragesCostDelta => MonthComparison?.FullComparisonDelta ?? 0;

    // New structured comparison
    public required MonthComparisonResult MonthComparison { get; set; }
}