namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Represents daily cost data containing a date and associated monetary cost amount.
/// DOMAIN: Core business value object representing cost data for a day
/// </summary>
public record DailyCostData
{
    public DateOnly DateString { get; set; }
    public decimal Cost { get; set; }
}