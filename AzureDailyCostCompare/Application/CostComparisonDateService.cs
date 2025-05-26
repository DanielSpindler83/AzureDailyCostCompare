using AzureDailyCostCompare.Application.Interfaces;
using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Manages date calculations for Azure cost comparisons between current and previous months.
/// Handles data availability cutoffs and determines appropriate comparison periods.
/// </summary>
public class CostComparisonDateService : ICostComparisonDateService
{
    /// <summary>
    /// The reference date used for all cost comparison calculations
    /// </summary>
    public DateTime CostDataReferenceDate { get; }

    /// <summary>
    /// First day of the previous month for cost comparison
    /// </summary>
    public DateTime PreviousMonthStartDate { get; }

    /// <summary>
    /// First day of the current month for cost comparison
    /// </summary>
    public DateTime CurrentMonthStartDate { get; }

    /// <summary>
    /// Total number of days in the previous month
    /// </summary>
    public int PreviousMonthDayCount { get; }

    /// <summary>
    /// Total number of days in the current month
    /// </summary>
    public int CurrentMonthDayCount { get; }

    /// <summary>
    /// Number of days to display in the comparison table
    /// </summary>
    public int ComparisonTableDayCount { get; private set; }

    /// <summary>
    /// UTC hour after which previous day's data is considered complete
    /// </summary>
    public int DataAvailabilityCutoffHourUtc { get; private set; }

    private CostComparisonDateService(int cutoffHourUtc, DateTime referenceDate)
    {
        CostDataReferenceDate = referenceDate;
        DataAvailabilityCutoffHourUtc = cutoffHourUtc;

        PreviousMonthStartDate = GetFirstDayOfMonth(referenceDate).AddMonths(-1);
        CurrentMonthStartDate = GetFirstDayOfMonth(referenceDate);
        PreviousMonthDayCount = DateTime.DaysInMonth(PreviousMonthStartDate.Year, PreviousMonthStartDate.Month);
        CurrentMonthDayCount = DateTime.DaysInMonth(CurrentMonthStartDate.Year, CurrentMonthStartDate.Month);
    }

    /// <summary>
    /// Creates a service instance using the current date and data availability cutoff
    /// </summary>
    /// <param name="cutoffHourUtc">UTC hour after which previous day's data is complete</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    public CostComparisonDateService(int cutoffHourUtc, IDateProvider? dateProvider = null)
        : this(cutoffHourUtc, DetermineLatestAvailableDataDate((dateProvider ?? new UtcDateProvider()).GetCurrentDate(), cutoffHourUtc))
    {
        ComparisonTableDayCount = CostDataReferenceDate.Day;
    }

    /// <summary>
    /// Creates a service instance for a specific override date
    /// </summary>
    /// <param name="cutoffHourUtc">UTC hour after which previous day's data is complete</param>
    /// <param name="overrideDate">Specific date to use for comparison calculations</param>
    /// <param name="dateProvider">Optional date provider for testing</param>
    public CostComparisonDateService(int cutoffHourUtc, DateTime overrideDate, IDateProvider? dateProvider = null)
        : this(cutoffHourUtc, ProcessOverrideDate(ValidateOverrideDate(overrideDate, cutoffHourUtc), (dateProvider ?? new UtcDateProvider()).GetCurrentDate()))
    {
        var currentDate = (dateProvider ?? new UtcDateProvider()).GetCurrentDate();
        ComparisonTableDayCount = CalculateComparisonDayCount(overrideDate, currentDate);
    }

    /// <summary>
    /// Creates a service instance for testing with specific date parameters
    /// </summary>
    public static CostComparisonDateService CreateForTesting(int cutoffHourUtc, int year, int month, int day)
    {
        return new CostComparisonDateService(cutoffHourUtc, new DateTime(year, month, day));
    }

    /// <summary>
    /// Determines if cost data is available for a given date based on the cutoff time
    /// </summary>
    public bool IsCostDataAvailable(DateTime date)
    {
        return date.Date <= CostDataReferenceDate.Date;
    }

    /// <summary>
    /// Gets the date range for the previous month's cost data
    /// </summary>
    public (DateTime StartDate, DateTime EndDate) GetPreviousMonthDateRange()
    {
        var endDate = PreviousMonthStartDate.AddMonths(1).AddDays(-1);
        return (PreviousMonthStartDate, endDate);
    }

    /// <summary>
    /// Gets the date range for the current month's cost data up to the reference date
    /// </summary>
    public (DateTime StartDate, DateTime EndDate) GetCurrentMonthDateRange()
    {
        var monthEndDate = CurrentMonthStartDate.AddMonths(1).AddDays(-1);
        var endDate = CostDataReferenceDate < monthEndDate ? CostDataReferenceDate : monthEndDate;
        return (CurrentMonthStartDate, endDate);
    }

    private static DateTime GetFirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    private static DateTime DetermineLatestAvailableDataDate(DateTime referenceDateTime, int cutoffHourUtc)
    {
        // Before cutoff: we don't have complete data for today, so last complete day is 2 days ago
        // After cutoff: yesterday's data should be complete
        DateTime latestCompleteDataDate = referenceDateTime.Hour < cutoffHourUtc
            ? referenceDateTime.Date.AddDays(-2)
            : referenceDateTime.Date.AddDays(-1);

        return latestCompleteDataDate;
    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate, int cutoffHourUtc)
    {
        DateTime latestAvailableDate = DetermineLatestAvailableDataDate(DateTime.UtcNow, cutoffHourUtc);

        if (overrideDate.Date > latestAvailableDate.Date)
        {
            throw new ArgumentException(
                $"Override date {overrideDate:yyyy-MM-dd} exceeds the latest date with complete cost data " +
                $"({latestAvailableDate:yyyy-MM-dd}). Data completeness is determined by the {cutoffHourUtc}:00 UTC cutoff.",
                nameof(overrideDate));
        }

        return overrideDate.Date;
    }

    private static DateTime ProcessOverrideDate(DateTime overrideDate, DateTime currentDate)
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

    private static int CalculateComparisonDayCount(DateTime overrideDate, DateTime currentDate)
    {
        if (overrideDate.Month == currentDate.Month && overrideDate.Year == currentDate.Year)
        {
            // For current month comparisons, use the current day of month
            return currentDate.Day;
        }

        // For historical month comparisons, use the maximum days to show full comparison
        int overrideMonthDays = DateTime.DaysInMonth(overrideDate.Year, overrideDate.Month);
        DateTime previousMonth = overrideDate.AddMonths(-1);
        int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

        return Math.Max(overrideMonthDays, previousMonthDays);
    }
}