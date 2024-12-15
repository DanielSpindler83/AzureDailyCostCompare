namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }

    public DateHelperService(
        DateTime? overrideDate = null,
        IDateProvider? dateProvider = null)
    {
        // Determine the date provider - we only inject a date provider for testing purposes
        var provider = dateProvider ?? new UtcDateProvider();

        // Get the reference datetime
        var referenceDateTime = overrideDate ?? provider.GetCurrentDate();

        // NOTE 6am is a educated approximation regarding MS Cost API having the full days data complete and available
        // Determine the data reference date, accounting for the 6am UTC cutoff to ensure a complete day data set
        // Once we pass 6am UTC - the full cost data is available for the date prior to todays UTC date
        // If no override date, apply the 6am UTC cutoff logic
        DataReferenceDate = overrideDate.HasValue
            ? ValidateOverrideDate(overrideDate.Value)
            : (referenceDateTime.Hour < 6
                ? referenceDateTime.Date.AddDays(-2)  // Before 6am UTC, we dont have full day today so last full day is 2 days ago
                : referenceDateTime.Date.AddDays(-1)); // its after 6am UTC so yesterday is last full day

        FirstDayOfPreviousMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }

    public string GetDataCurrencyDescription(TimeZoneInfo? localTimeZone = null)
    {
        localTimeZone ??= TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(DataReferenceDate, localTimeZone);

        if (DataReferenceDate.Day == 1) // NOTE this covers up to 5:59am UTC on 2nd (due to already accounting for 6am UTC full day data set)
        {
            // not sure if really need this? If we do is there a way we can deduplicate the string data?
            return $"Today is between 1st of the month 6am and 2nd of the month 5:59am UTC, showing last two full months(all days complete data)." +
                   $"Daily cost data is complete up to {DataReferenceDate} UTC\n" +
                   $"In your local timezone ({localTimeZone.DisplayName}): {localDataReferenceDay}";
        }
        else
        {
            return $"Daily cost data is complete up to {DataReferenceDate} UTC\n" +
            $"In your local timezone ({localTimeZone.DisplayName}): {localDataReferenceDay}";
        }

    }

    private static DateTime ValidateOverrideDate(DateTime overrideDate)
    {
        // Get the current UTC time
        var currentUtcTime = DateTime.UtcNow;

        // Determine the latest date with full data available based on 6am UTC cutoff
        DateTime latestFullDataDate = currentUtcTime.Hour < 6
            ? currentUtcTime.Date.AddDays(-2)  // Before 6am, full data is available two days back
            : currentUtcTime.Date.AddDays(-1); // After 6am, full data is available for previous day

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

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}