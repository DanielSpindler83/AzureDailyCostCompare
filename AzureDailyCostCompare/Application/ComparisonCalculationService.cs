using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Determines the type of comparison and calculates comparison day counts
/// APPLICATION: Service that applies comparison business rules
/// </summary>
public class ComparisonCalculationService
{

    private DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Determines the type of comparison based on override and current dates
    /// </summary>
    public ComparisonType DetermineComparisonType(DateTime date)
    {
        return date.Month == CurrentDateTimeUtc.Month && date.Year == CurrentDateTimeUtc.Year
            ? ComparisonType.PartialMonth
            : ComparisonType.FullMonth;
    }

    /// <summary>
    /// Calculates the number of days to display in the comparison table
    /// </summary>
    /// <param name="overrideDate">Override date (null for current date scenario)</param>
    /// <param name="currentDate">Current date</param>
    /// <param name="referenceDate">Reference date used for calculations</param>
    /// <returns>Number of days for comparison table</returns>
    public int CalculateComparisonDayCount(DateTime date, ComparisonType comparisonType)
    {
        if (comparisonType == ComparisonType.PartialMonth)
        {
            return date.Day;
        }

        // Historical month comparisons - use maximum days to show comparison of ALL days in the month (even though user may have asked for a specific date)
        int overrideMonthDays = DateTime.DaysInMonth(date.Year, date.Month);
        DateTime previousMonth = date.AddMonths(-1);
        int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

        return Math.Max(overrideMonthDays, previousMonthDays);
    }

    public DateTime AdjustForHistoricalDate(DateTime date)
    {
        // If the date represents a complete month in the past,
        // adjust it to the last day of that month for full month comparison
        bool isCompleteMonthInPast = date < CurrentDateTimeUtc &&
                                   date.Month != CurrentDateTimeUtc.Month &&
                                   date.Year <= CurrentDateTimeUtc.Year;

        if (isCompleteMonthInPast)
        {
            return new DateTime(date.Year, date.Month,
                              DateTime.DaysInMonth(date.Year, date.Month));
        }

        return date;
    }
}