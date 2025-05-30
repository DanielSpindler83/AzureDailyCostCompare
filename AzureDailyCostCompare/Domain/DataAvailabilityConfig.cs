namespace AzureDailyCostCompare.Domain;

/// <summary>
/// Configuration for data availability calculations
/// DOMAIN: Business rules for when data is considered complete
/// </summary>
public record DataAvailabilityConfig(int CutoffHourUtc)
{
    public int CutoffHourUtc { get; } = CutoffHourUtc is >= 0 and <= 23
        ? CutoffHourUtc
        : throw new ArgumentOutOfRangeException(nameof(CutoffHourUtc), "Must be between 0 and 23");
}