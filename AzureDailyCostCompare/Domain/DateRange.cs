namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Represents a date range with start and end dates
/// DOMAIN: Core business value object for date ranges
/// </summary>
public record DateRange(DateTime Start, DateTime End)
{
    /// <summary>Number of days in the range (inclusive)</summary>
    public int DayCount => (End - Start).Days + 1;
}
