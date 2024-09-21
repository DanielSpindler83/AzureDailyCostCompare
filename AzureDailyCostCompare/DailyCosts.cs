namespace AzureDailyCostCompare;

public class DailyCosts
{
    public decimal Cost { get; set; }
    public DateOnly DateString { get; set; }
    public string Currency { get; set; } = string.Empty;
}
