

using Azure.Identity;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Handles determination of when cost data is considered complete and available
/// APPLICATION: Service that applies domain rules using infrastructure
/// </summary>
public class DataAvailabilityService
{
    private DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Determines the latest date for which a full days cost data is considered available
    /// </summary>
    /// <param name="currentDateTime">Current date and time</param>
    /// <param name="previousDayUtcDataLoadDelayHours">Hours after UTC midnight to declare previous days data complete enough to load</param>
    /// <returns>Latest date with complete cost data</returns>
    public DateTime GetLatestAvailableFullDaysDataDate(int previousDayUtcDataLoadDelayHours)
    {
        // Before cutoff: we don't have complete data for yesterday, so last complete day is the day before yesterday
        // After cutoff: yesterday's data is considered a full days data
        return CurrentDateTimeUtc.Hour < previousDayUtcDataLoadDelayHours
            ? CurrentDateTimeUtc.Date.AddDays(-2) // we only use the ComparisonReferenceDate portion of this and never the time - maybe we should return just a ComparisonReferenceDate (with no time)
            : CurrentDateTimeUtc.Date.AddDays(-1);
        //return CurrentDateTimeUtc.Hour < previousDayUtcDataLoadDelayHours
        //    ? DateOnly.FromDateTime(CurrentDateTimeUtc.ComparisonReferenceDate.AddDays(-2)) // we only use the ComparisonReferenceDate portion of this and never the time - maybe we should return just a ComparisonReferenceDate (with no time)
        //    : DateOnly.FromDateTime(CurrentDateTimeUtc.ComparisonReferenceDate.AddDays(-1));
    }

    /// <summary>
    /// Validates that an override date doesn't exceed available data
    /// </summary>
    /// <param name="overrideDate">ComparisonReferenceDate to validate</param>
    /// <param name="currentDateTime">Current date and time</param>
    /// <param name="previousDayUtcDataLoadDelayHours">Hours after UTC midnight to declare previous days data complete enough to load</param>
    /// <returns>Validated date (normalized to date only)</returns>
    /// <exception cref="ArgumentException">When override date exceeds available data</exception>
    public DateTime ValidateDate(DateTime comparisonReferenceDate, DateTime latestAvailableFullDaysDataDate, int previousDayUtcDataLoadDelayHours)
    {

        if (comparisonReferenceDate.Date > latestAvailableFullDaysDataDate)
        {
            throw new ArgumentException(
                $"ComparisonReferenceDate {comparisonReferenceDate:yyyy-MM-dd} exceeds the latest date with complete cost data " +
                $"({latestAvailableFullDaysDataDate:yyyy-MM-dd}). Data completeness is determined by the {previousDayUtcDataLoadDelayHours}:00 UTC cutoff.",
                nameof(comparisonReferenceDate)); // review error message
        }

        return comparisonReferenceDate.Date;
    }
}
