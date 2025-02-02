namespace AzureDailyCostCompare;

public record DailyCostData
{
    public DateOnly DateString { get; set; }
    public decimal Cost { get; set; }
}