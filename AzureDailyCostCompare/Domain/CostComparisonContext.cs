using AzureDailyCostCompare.Application;

namespace AzureDailyCostCompare.Domain;

public record CostComparisonContext
{
    public DateTime ComparisonReferenceDate { get; init; }
    public MonthComparisonPeriod MonthComparisonPeriod { get; init; }
    public int ComparisonTableDayCount { get; init; }
    public int DataLoadDelayHours { get; init; }

    public CostComparisonContext(
        DateTime comparisonReferenceDate,
        MonthComparisonPeriod monthComparisonPeriod,
        int comparisonTableDayCount,
        int dataLoadDelayHours)
    {
        ComparisonReferenceDate = comparisonReferenceDate;
        MonthComparisonPeriod = monthComparisonPeriod;
        ComparisonTableDayCount = comparisonTableDayCount;
        DataLoadDelayHours = dataLoadDelayHours;
    }
}