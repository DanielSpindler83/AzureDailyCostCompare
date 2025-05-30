using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Determines the type of comparison and calculates comparison day counts
/// APPLICATION: Service that applies comparison business rules
/// </summary>
public class ComparisonCalculationService
{
    /// <summary>
    /// Determines the type of comparison based on override and current dates
    /// </summary>
    public ComparisonType DetermineComparisonType(DateTime overrideDate, DateTime currentDate)
    {
        return overrideDate.Month == currentDate.Month && overrideDate.Year == currentDate.Year
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
    public int CalculateComparisonDayCount(DateTime? overrideDate, DateTime currentDate, DateTime referenceDate)
    {
        // No override date - use current day of month
        if (!overrideDate.HasValue)
        {
            return referenceDate.Day;
        }

        // Override date in current month - use current day of month  
        if (overrideDate.Value.Month == currentDate.Month && overrideDate.Value.Year == currentDate.Year)
        {
            return currentDate.Day;
        }

        // Historical month comparisons - use maximum days to show full comparison
        int overrideMonthDays = DateTime.DaysInMonth(overrideDate.Value.Year, overrideDate.Value.Month);
        DateTime previousMonth = overrideDate.Value.AddMonths(-1);
        int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

        return Math.Max(overrideMonthDays, previousMonthDays);
    }
}