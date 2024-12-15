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

        // NOTE if we set historical date to be 1st UTC with midnight time(the default) - do we have a bug? What does it show? last 2 months without last day of month....not ideal
        // For historical if we want to see two full months(all days complete) - set datetime between 1st of the month 6am and 2nd of the month 5:59am

        // NOTE 6am is a educated guess regarding MS Cost API having the full days data mostly done and available
        // Determine the data reference date, accounting for the 6am UTC cutoff to ensure a complete day data set
        // Once we pass 6am UTC - the full cost data is available for the date prior to todays UTC date
        DataReferenceDate = referenceDateTime.Hour < 6
            ? referenceDateTime.Date.AddDays(-2)  // Before 6am UTC, we dont have full day today so last full day is 2 days ago
            : referenceDateTime.Date.AddDays(-1); // its after 6am UTC so yesterday is last full day

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
            return $"Today is between 1st of the month 6am and 2nd of the month 5:59am, showing last two full months(all days complete data)." +
                   $"Daily cost data is complete up to {DataReferenceDate:yyyy-MM-dd} at 06:00 UTC\n" +
                   $"In your local timezone ({localTimeZone.DisplayName}): {localDataReferenceDay}";
        }
        else
        {
            return $"Daily cost data is complete up to {DataReferenceDate:yyyy-MM-dd} at 06:00 UTC\n" +
            $"In your local timezone ({localTimeZone.DisplayName}): {localDataReferenceDay}";
        }

    }

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}