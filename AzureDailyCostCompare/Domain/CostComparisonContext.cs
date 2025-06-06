namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Contains all calculated date information for cost comparison
/// DOMAIN: Aggregate root containing all comparison context data
/// </summary>
public record CostComparisonContext
{
    public DateTime ComparisonReferenceDate { get; init; }
    public ComparisonType ComparisonType { get; init; }
    public DateTime CurrentMonthStart { get; init; }
    public DateTime PreviousMonthStart { get; init; }
    public int CurrentMonthDayCount { get; init; }
    public int PreviousMonthDayCount { get; init; }
    public int ComparisonTableDayCount { get; init; }
    public int DataLoadDelayHours { get; init; }

    public CostComparisonContext(
        DateTime comparisonReferenceDate,
        ComparisonType comparisonType,
        DateTime currentMonthStart,
        DateTime previousMonthStart,
        int currentMonthDayCount,
        int previousMonthDayCount,
        int comparisonTableDayCount,
        int dataLoadDelayHours)
    {
        ComparisonReferenceDate = comparisonReferenceDate;
        ComparisonType = comparisonType;
        CurrentMonthStart = currentMonthStart;
        PreviousMonthStart = previousMonthStart;
        CurrentMonthDayCount = currentMonthDayCount;
        PreviousMonthDayCount = previousMonthDayCount;
        ComparisonTableDayCount = comparisonTableDayCount;
        DataLoadDelayHours = dataLoadDelayHours;
    }
    /// <summary>Gets the date range for the previous month</summary>
    public DateRange GetPreviousMonthRange()
    {
        var endDate = PreviousMonthStart.AddMonths(1).AddDays(-1);
        return new DateRange(PreviousMonthStart, endDate);
    }

    /// <summary>Gets the date range for the current month up to reference date</summary>
    public DateRange GetCurrentMonthRange()
    {
        var monthEndDate = CurrentMonthStart.AddMonths(1).AddDays(-1);
        var endDate = ComparisonReferenceDate < monthEndDate ? ComparisonReferenceDate : monthEndDate;
        return new DateRange(CurrentMonthStart, endDate);
    }

    /// <summary>Determines if cost data is available for a given date</summary>
    public bool IsCostDataAvailable(DateTime date) => date.Date <= ComparisonReferenceDate.Date;
}