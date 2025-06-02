using AzureDailyCostCompare.Domain;
using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class ReportGenerator
{
    private readonly CostDataProcessor _costDataProcessor;
    private readonly IReportRenderer _reportRenderer;

    public ReportGenerator(CostDataProcessor costDataProcessor, IReportRenderer reportRenderer)
    {
        _costDataProcessor = costDataProcessor;
        _reportRenderer = reportRenderer;
    }

    public void GenerateDailyCostReport(
        List<DailyCostData> costData,
        CostComparisonContext costComparisonContext,
        bool showWeeklyPatterns,
        bool showDayOfWeekAverages)
    {
        var processedData = _costDataProcessor.ProcessCostData(costData, costComparisonContext);

        _reportRenderer.RenderDailyCostTable(processedData, costComparisonContext);

        if (showWeeklyPatterns)
            _reportRenderer.RenderWeeklyComparisons(processedData, costComparisonContext);

        if (showDayOfWeekAverages)
            _reportRenderer.RenderDayOfWeekAverages(processedData, costComparisonContext);

        _reportRenderer.RenderDataAnalysisAndInfo(processedData, costComparisonContext);
    }
}