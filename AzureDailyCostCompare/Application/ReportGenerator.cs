using AzureDailyCostCompare.Domain;
using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class ReportGenerator(IReportRenderer reportRenderer)
{
    public void GenerateDailyCostReport(
        List<DailyCostData> costData,
        CostComparisonContext costComparisonContext,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages)
    {
        var processedData = CostDataProcessor.ProcessCostData(costData, costComparisonContext);

        reportRenderer.RenderDailyCostTable(processedData, costComparisonContext);

        if (showWeeklyPatterns)
            reportRenderer.RenderWeeklyComparisons(processedData, costComparisonContext);

        if (showDayOfWeekAverages)
            reportRenderer.RenderDayOfWeekAverages(processedData, costComparisonContext);

        reportRenderer.RenderDataAnalysisAndInfo(processedData, costComparisonContext);
    }
}