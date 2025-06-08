namespace AzureDailyCostCompare.Application;

public class DataAvailabilityService
{
    private DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    public DateTime GetLatestAvailableFullDaysDataDate(int previousDayUtcDataLoadDelayHours)
    {
        // Before cutoff: we don't have complete data for yesterday, so last complete day is the day before yesterday
        // After cutoff: yesterday's data is considered a full days data
        return CurrentDateTimeUtc.Hour < previousDayUtcDataLoadDelayHours
            ? CurrentDateTimeUtc.Date.AddDays(-2) // we only use the InputComparisonDate portion of this and never the time - maybe we should return just a InputComparisonDate (with no time)
            : CurrentDateTimeUtc.Date.AddDays(-1);
    }


    public DateTime ValidateDate(DateTime inputComparisonDate, DateTime latestAvailableFullDaysDataDate)
    {

        if (inputComparisonDate > latestAvailableFullDaysDataDate)
        {
            // maybe we should log this or tell the user?
            return latestAvailableFullDaysDataDate.Date;
        }

        return inputComparisonDate;
    }
}
