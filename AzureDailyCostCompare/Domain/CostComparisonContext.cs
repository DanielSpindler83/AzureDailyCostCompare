namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Contains all calculated date information for cost comparison
/// DOMAIN: Aggregate root containing all comparison context data
/// </summary>
public record CostComparisonContext(
    DateTime ComparisonReferenceDate,
    ComparisonType ComparisonType,
    DateTime CurrentMonthStart,
    DateTime PreviousMonthStart,
    int CurrentMonthDayCount,
    int PreviousMonthDayCount,
    int ComparisonTableDayCount,
    int DataLoadDelayHours)
{
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