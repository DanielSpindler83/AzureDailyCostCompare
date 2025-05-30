using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

/// <summary>
/// Handles determination of when cost data is considered complete and available
/// APPLICATION: Service that applies domain rules using infrastructure
/// </summary>
public class DataAvailabilityService
{
    /// <summary>
    /// Determines the latest date for which complete cost data should be available
    /// </summary>
    /// <param name="currentDateTime">Current date and time</param>
    /// <param name="config">Data availability configuration</param>
    /// <returns>Latest date with complete cost data</returns>
    public DateTime GetLatestAvailableDataDate(DateTime currentDateTime, DataAvailabilityConfig config)
    {
        // Before cutoff: we don't have complete data for today, so last complete day is 2 days ago
        // After cutoff: yesterday's data should be complete
        return currentDateTime.Hour < config.CutoffHourUtc
            ? currentDateTime.Date.AddDays(-2)
            : currentDateTime.Date.AddDays(-1);
    }

    /// <summary>
    /// Validates that an override date doesn't exceed available data
    /// </summary>
    /// <param name="overrideDate">Date to validate</param>
    /// <param name="currentDateTime">Current date and time</param>
    /// <param name="config">Data availability configuration</param>
    /// <returns>Validated date (normalized to date only)</returns>
    /// <exception cref="ArgumentException">When override date exceeds available data</exception>
    public DateTime ValidateOverrideDate(DateTime overrideDate, DateTime currentDateTime, DataAvailabilityConfig config)
    {
        DateTime latestAvailableDate = GetLatestAvailableDataDate(currentDateTime, config);

        if (overrideDate.Date > latestAvailableDate.Date)
        {
            throw new ArgumentException(
                $"Override date {overrideDate:yyyy-MM-dd} exceeds the latest date with complete cost data " +
                $"({latestAvailableDate:yyyy-MM-dd}). Data completeness is determined by the {config.CutoffHourUtc}:00 UTC cutoff.",
                nameof(overrideDate));
        }

        return overrideDate.Date;
    }
}
