using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Infrastructure;

public class ReportGeneratorFactory
{
    private readonly PreviousDayUtcDataLoadDelayHours _previousDayUtcDataLoadDelayHours;

    public ReportGeneratorFactory(PreviousDayUtcDataLoadDelayHours previousDayUtcDataLoadDelayHours)
    {
        _previousDayUtcDataLoadDelayHours = previousDayUtcDataLoadDelayHours;
    }

    public ReportGenerator Create(List<DailyCostData> costData, CostComparisonContext context)
    {
        return new ReportGenerator(_previousDayUtcDataLoadDelayHours, costData, context);
    }
}