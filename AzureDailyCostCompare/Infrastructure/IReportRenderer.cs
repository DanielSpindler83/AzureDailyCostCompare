using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Infrastructure;

public interface IReportRenderer
{
    void RenderDailyCostTable(ProcessedCostData data, CostComparisonContext context);
    void RenderWeeklyComparisons(ProcessedCostData data, CostComparisonContext context);
    void RenderDayOfWeekAverages(ProcessedCostData data, CostComparisonContext context);
    void RenderDataAnalysisAndInfo(ProcessedCostData data, CostComparisonContext context);
}