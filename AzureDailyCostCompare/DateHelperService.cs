namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }

    public const int FULL_DAY_DATA_CUTOFF_HOUR = 4; // NOTE 4am is a educated approximation(based on testing) regarding MS Cost API having the full previous days data complete and available

    public DateHelperService(
        DateTime? overrideDate = null,
        IDateProvider? dateProvider = null)
    {
        var provider = dateProvider ?? new UtcDateProvider();

        var referenceDateTime = overrideDate.HasValue
            ? ValidateOverrideDate(overrideDate.Value)
            : provider.GetCurrentDate();

        DataReferenceDate = overrideDate ?? DetermineDataReferenceDate(referenceDateTime);

        FirstDayOfPreviousMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }

    public string GetDataCurrencyDescription(TimeZoneInfo? localTimeZone = null)
    {
        localTimeZone ??= TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(DataReferenceDate, localTimeZone);

        return $"Daily cost data is complete up to end of the day {DataReferenceDate:yyyy-MM-dd} in UTC timezone\n" +
               $"The end of the day in UTC time is {localDataReferenceDay} in local timezone of {localTimeZone.DisplayName}\n" +
               $"------\n" +
               $"This report was generated at {DateTime.Now} {localTimeZone.DisplayName}\n" +
               $"------\n";
    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate)
    {

        DateTime latestFullDataDate = DetermineDataReferenceDate(DateTime.UtcNow);

        if (overrideDate.Date > latestFullDataDate.Date)
        {
            throw new ArgumentException(
                $"Override date must be on or before {latestFullDataDate:yyyy-MM-dd} " +
                $"to ensure full data availability (based on {FULL_DAY_DATA_CUTOFF_HOUR} o'clock UTC cutoff).",
                nameof(overrideDate));
        }

        return overrideDate.Date;
    }

    private static DateTime DetermineDataReferenceDate(DateTime referenceDateTime)
    {
        DateTime selectedDate = referenceDateTime.Hour < FULL_DAY_DATA_CUTOFF_HOUR
            ? referenceDateTime.Date.AddDays(-2)  // Before cutoff, we dont have full day today so last full day is 2 days ago
            : referenceDateTime.Date.AddDays(-1); // its after cutoff so yesterday is last full day

        return selectedDate.Date;
    }

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}