using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class ProcessedCostData
{
    public List<DailyCostData> CurrentMonthCostData { get; set; } = [];
    public List<DailyCostData> PreviousMonthCostData { get; set; } = [];
    public decimal AverageCurrentPartialMonth { get; set; }
    public decimal AveragePreviousPartialMonth { get; set; }
    public decimal AveragePreviousFullMonth { get; set; }
    public decimal CurrentToPreviousMonthAveragesCostDelta { get; set; }
}