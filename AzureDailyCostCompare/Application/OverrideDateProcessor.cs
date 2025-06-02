namespace AzureDailyCostCompare.Application;

/// <summary>
/// Handles date processing logic for override dates
/// APPLICATION: Service that applies business rules for date processing
/// </summary>
public class OverrideDateProcessor
{
    /// <summary>
    /// Processes an override date, adjusting it for full month comparisons when appropriate
    /// </summary>
    /// <param name="overrideDate">The override date to process</param>
    /// <param name="currentDate">Current date for comparison</param>
    /// <returns>Processed date (potentially adjusted to end of month)</returns>
    public DateTime ProcessOverrideDate(DateTime overrideDate, DateTime currentDate)
    {
        // If the override date represents a complete month in the past,
        // adjust it to the last day of that month for full month comparison
        bool isCompleteMonthInPast = overrideDate < currentDate &&
                                   overrideDate.Month != currentDate.Month &&
                                   overrideDate.Year <= currentDate.Year;

        if (isCompleteMonthInPast)
        {
            return new DateTime(overrideDate.Year, overrideDate.Month,
                              DateTime.DaysInMonth(overrideDate.Year, overrideDate.Month));
        }

        return overrideDate;
    }
}