using AzureDailyCostCompare.Application;

namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Contains all calculated date information for cost comparison
/// DOMAIN: Aggregate root containing all comparison context data
/// </summary>
public record CostComparisonContext
{
    public DateTime ComparisonReferenceDate { get; init; }
    public ComparisonType ComparisonType { get; init; }
    public MonthComparisonPeriod MonthComparisonPeriod { get; init; }
    public int ComparisonTableDayCount { get; init; }
    public int DataLoadDelayHours { get; init; }

    public CostComparisonContext(
        DateTime comparisonReferenceDate,
        ComparisonType comparisonType,
        MonthComparisonPeriod monthComparisonPeriod,
        int comparisonTableDayCount,
        int dataLoadDelayHours)
    {
        ComparisonReferenceDate = comparisonReferenceDate;
        ComparisonType = comparisonType;
        MonthComparisonPeriod = monthComparisonPeriod;
        ComparisonTableDayCount = comparisonTableDayCount;
        DataLoadDelayHours = dataLoadDelayHours;
    }
}

//ComparisonReferenceDate = comparisonReferenceDate;
//        ComparisonType = comparisonType;
//        CurrentMonthStart = monthComparisonPeriod.CurrentFirstDayOfMonth;
//        PreviousMonthStart = monthComparisonPeriod.PreviousFirstDayOfMonth;
//        CurrentMonthDayCount = monthComparisonPeriod.CurrentMonthDaysCount;
//        PreviousMonthDayCount = monthComparisonPeriod.PreviousMonthDaysCount;
//        ComparisonTableDayCount = comparisonTableDayCount;
//        DataLoadDelayHours = dataLoadDelayHours;