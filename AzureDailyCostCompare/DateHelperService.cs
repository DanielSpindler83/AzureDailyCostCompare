namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }
    public int OutputTableDaysToDisplay { get; private set; }

    public const int FULL_DAY_DATA_CUTOFF_HOUR_UTC = 4; // NOTE 4am is a educated approximation(based on testing) regarding MS Cost API having the full previous days data complete and available

    private DateHelperService(DateTime referenceDate)
    {
        DataReferenceDate = referenceDate;

        FirstDayOfPreviousMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);

    }

    // Constructor for current date
    public DateHelperService(IDateProvider? dateProvider = null)
        : this(DetermineDataReferenceDate((dateProvider ?? new UtcDateProvider()).GetCurrentDate()))
    {
        OutputTableDaysToDisplay = DataReferenceDate.Day;
    }

    // Constructor for override date
    public DateHelperService(DateTime overrideDate, IDateProvider? dateProvider = null)
        : this(AdjustDateForFullMonthInPast(ValidateOverrideDate(overrideDate), (dateProvider ?? new UtcDateProvider()).GetCurrentDate()))
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

    private static DateTime DetermineDataReferenceDate(DateTime referenceDateTime)
    {
        DateTime selectedDate = referenceDateTime.Hour < FULL_DAY_DATA_CUTOFF_HOUR_UTC
            ? referenceDateTime.Date.AddDays(-2)  // Before cutoff, we don't have full day today so last full day is 2 days ago
            : referenceDateTime.Date.AddDays(-1); // It's after cutoff so yesterday is last full day

        return selectedDate.Date;
    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate)
    {
        DateTime latestFullDataDate = DetermineDataReferenceDate(DateTime.UtcNow);

        if (overrideDate.Date > latestFullDataDate.Date)
        {
            throw new ArgumentException(
                $"Override date must be on or before {latestFullDataDate:yyyy-MM-dd} " +
                $"to ensure full data availability (based on {FULL_DAY_DATA_CUTOFF_HOUR_UTC} o'clock UTC cutoff).",
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

    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}