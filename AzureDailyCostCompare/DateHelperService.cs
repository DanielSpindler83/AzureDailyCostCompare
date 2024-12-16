namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }

    private const int FULL_DAY_DATA_CUTOFF_HOUR = 4; // NOTE 4am is a educated approximation regarding MS Cost API having the full days data complete and available

    public DateHelperService(
        DateTime? overrideDate = null,
        IDateProvider? dateProvider = null)
    {
        // Determine the date provider - we only inject a date provider for testing purposes
        var provider = dateProvider ?? new UtcDateProvider();

        // Get the reference datetime
        var referenceDateTime = overrideDate ?? provider.GetCurrentDate();

        // If no override date, apply the Xam UTC cutoff logic
        DataReferenceDate = overrideDate.HasValue
            ? ValidateOverrideDate(overrideDate.Value)
            : DetermineDataReferenceDate(referenceDateTime);

        FirstDayOfPreviousMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }

    public string GetDataCurrencyDescription(TimeZoneInfo? localTimeZone = null)
    {
        localTimeZone ??= TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(DataReferenceDate, localTimeZone);

        return $"Daily cost data is complete up to {DataReferenceDate} UTC\n" +
               $"Daily cost data is complete up to {localDataReferenceDay} {localTimeZone.DisplayName}\n" +
               $"------\n" +
               $"This report was generated at {DateTime.Now} {localTimeZone.DisplayName}\n" +
               $"------\n";
    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate)
    {
        // Determine the latest date with full data available based on reference cut off
        DateTime latestFullDataDate = DetermineDataReferenceDate(DateTime.UtcNow);

        // Ensure the override date is not in the future and has full data available
        if (overrideDate.Date > latestFullDataDate.Date)
        {
            throw new ArgumentException(
                $"Override date must be on or before {latestFullDataDate:yyyy-MM-dd} " +
                "to ensure full data availability (based on 6am UTC cutoff).",
                nameof(overrideDate));
        }

        // Return the date with time stripped
        return overrideDate.Date;
    }

    private static DateTime DetermineDataReferenceDate(DateTime referenceDateTime)
    {
        DateTime selectedDate = referenceDateTime.Hour < FULL_DAY_DATA_CUTOFF_HOUR
            ? referenceDateTime.Date.AddDays(-2)  // Before cutoff, we dont have full day today so last full day is 2 days ago
            : referenceDateTime.Date.AddDays(-1); // its after cutoff so yesterday is last full day

        return selectedDate.Date.AddHours(FULL_DAY_DATA_CUTOFF_HOUR); // set time to be the cutoff so when we display it as date time it is to when the data is current to
    }

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}