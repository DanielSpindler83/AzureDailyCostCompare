using AzureDailyCostCompare.Infrastructure;

namespace AzureDailyCostCompare.Application;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }
    public int OutputTableDaysToDisplay { get; private set; }
    public int PreviousDayUtcDataLoadDelayHours { get; private set; }

    private DateHelperService(int cutoffHourUtc, DateTime referenceDate)
    {
        DataReferenceDate = referenceDate;
        PreviousDayUtcDataLoadDelayHours = cutoffHourUtc;

        FirstDayOfPreviousMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);

    }

    // Constructor for current date
    public DateHelperService(int cutoffHourUtc, IDateProvider? dateProvider = null)
        : this(cutoffHourUtc, DetermineDataReferenceDate((dateProvider ?? new UtcDateProvider()).GetCurrentDate(), cutoffHourUtc))
    {
        OutputTableDaysToDisplay = DataReferenceDate.Day;
    }

    // Constructor for override date
    public DateHelperService(int cutoffHourUtc, DateTime overrideDate, IDateProvider? dateProvider = null)
        : this(cutoffHourUtc, AdjustDateForFullMonthInPast(ValidateOverrideDate(overrideDate, cutoffHourUtc), (dateProvider ?? new UtcDateProvider()).GetCurrentDate()))
    {
        var currentDate = (dateProvider ?? new UtcDateProvider()).GetCurrentDate();

        if (overrideDate.Month == currentDate.Month && overrideDate.Year == currentDate.Year)
        {
            // If override date is in current month, use the current day of month
            OutputTableDaysToDisplay = currentDate.Day;
        }
        else
        {
            // For dates in previous months, use the larger of the two month's day counts
            int overrideMonthDays = DateTime.DaysInMonth(overrideDate.Year, overrideDate.Month);
            DateTime previousMonth = overrideDate.AddMonths(-1);
            int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            OutputTableDaysToDisplay = Math.Max(overrideMonthDays, previousMonthDays);
        }
    }

    public string GetDataCurrencyDescription(TimeZoneInfo? localTimeZone = null)
    {
        localTimeZone ??= TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(DataReferenceDate, localTimeZone);

        return $"Daily cost data is complete up to end of the day {DataReferenceDate:yyyy-MM-dd} in UTC timezone\n" +
               $"The end of the day in UTC time is {localDataReferenceDay} in local timezone of {localTimeZone.DisplayName}\n" +
               $"------\n" +
               $"This report was generated at {DateTime.Now} {localTimeZone.DisplayName}\n" +
               $"This report was generated at {DateTime.UtcNow} UTC\n" +
               $"------\n";
    }

    private static DateTime DetermineDataReferenceDate(DateTime referenceDateTime, int cutoffHourUtc)
    {
        DateTime selectedDate = referenceDateTime.Hour < cutoffHourUtc
            ? referenceDateTime.Date.AddDays(-2)  // Before cutoff, we don't have full day today so last full day is 2 days ago
            : referenceDateTime.Date.AddDays(-1); // It's after cutoff so yesterday is last full day

        return selectedDate.Date;
    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate, int cutoffHourUtc)
    {
        DateTime latestFullDataDate = DetermineDataReferenceDate(DateTime.UtcNow, cutoffHourUtc);

        if (overrideDate.Date > latestFullDataDate.Date)
        {
            throw new ArgumentException(
                $"Override date must be on or before {latestFullDataDate:yyyy-MM-dd} " +
                $"to ensure full data availability (based on {cutoffHourUtc} o'clock UTC cutoff).",
                nameof(overrideDate));
        }

        return overrideDate.Date;
    }

    private static DateTime AdjustDateForFullMonthInPast(DateTime overrideDate, DateTime currentDate)
    {
        // Check if the override date is in a full month in the past
        if (overrideDate < currentDate && overrideDate.Month != currentDate.Month && overrideDate.Year <= currentDate.Year)
        {
            // Adjust the date to the last day of the provided month
            return new DateTime(overrideDate.Year, overrideDate.Month, DateTime.DaysInMonth(overrideDate.Year, overrideDate.Month));
        }

        // Otherwise, use the provided date as-is
        return overrideDate;
    }

    public static DateHelperService CreateForTesting(int cutoffHourUtc, int year, int month, int day)
    {
        return new DateHelperService(cutoffHourUtc, new DateTime(year, month, day));
    }
}