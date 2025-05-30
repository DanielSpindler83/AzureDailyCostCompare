namespace AzureDailyCostCompare.Application;


/// <summary>
/// Calculates month-related date information
/// APPLICATION: Service for month boundary and day count calculations
/// </summary>
public class MonthCalculationService
{
    /// <summary>
    /// Gets the first day of the month for a given date
    /// </summary>
    public DateTime GetFirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Gets the number of days in a month for a given date
    /// </summary>
    public int GetDaysInMonth(DateTime date)
    {
        return DateTime.DaysInMonth(date.Year, date.Month);
    }

    /// <summary>
    /// Calculates month boundaries and day counts for a reference date
    /// </summary>
    public (DateTime CurrentStart, DateTime PreviousStart, int CurrentDays, int PreviousDays)
        CalculateMonthBoundaries(DateTime referenceDate)
    {
        var currentStart = GetFirstDayOfMonth(referenceDate);
        var previousStart = currentStart.AddMonths(-1);
        var currentDays = GetDaysInMonth(referenceDate);
        var previousDays = GetDaysInMonth(previousStart);

        return (currentStart, previousStart, currentDays, previousDays);
    }
}