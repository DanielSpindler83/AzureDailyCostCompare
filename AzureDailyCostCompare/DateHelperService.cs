namespace AzureDailyCostCompare;

public class DateHelperService
{
    public DateTime DataReferenceDate { get; }
    public DateTime FirstDayOfPreviousMonth { get; }
    public DateTime FirstDayOfCurrentMonth { get; }
    public int CountOfDaysInPreviousMonth { get; }
    public int CountOfDaysInCurrentMonth { get; }
    public DateTime LastCompleteCostDay { get; }

    public DateHelperService(
        DateTime? overrideDate = null,
        IDateProvider? dateProvider = null)
    {
        // Determine the date provider - we only inject a date provider for testing purposes
        var provider = dateProvider ?? new UtcDateProvider();

        // Get the reference datetime
        var referenceDateTime = overrideDate ?? provider.GetCurrentDate();

        // Determine the data reference date, accounting for the 6am UTC cutoff to ensure a complete day data set
        DataReferenceDate = referenceDateTime.Hour < 6
            ? referenceDateTime.Date.AddDays(-1)  // Before 6am UTC, use previous day
            : referenceDateTime.Date;

        FirstDayOfPreviousMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1).AddMonths(-1);
        FirstDayOfCurrentMonth = new DateTime(DataReferenceDate.Year, DataReferenceDate.Month, 1);
        CountOfDaysInPreviousMonth = DateTime.DaysInMonth(FirstDayOfPreviousMonth.Year, FirstDayOfPreviousMonth.Month);
        CountOfDaysInCurrentMonth = DateTime.DaysInMonth(FirstDayOfCurrentMonth.Year, FirstDayOfCurrentMonth.Month);
    }

    public string GetDataCurrencyDescription(TimeZoneInfo? localTimeZone = null)
    {
        localTimeZone ??= TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(DataReferenceDate, localTimeZone);
        return $"Daily cost data is complete up to {DataReferenceDate:yyyy-MM-dd} at 06:00 UTC\n" +
               $"In your local timezone ({localTimeZone.DisplayName}): {localDataReferenceDay:yyyy-MM-dd}";
    }

    // Static method for easy testing creation
    public static DateHelperService CreateForTesting(int year, int month, int day)
    {
        return new DateHelperService(new DateTime(year, month, day));
    }
}