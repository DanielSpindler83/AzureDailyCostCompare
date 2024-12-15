namespace AzureDailyCostCompare;

record DailyCosts
{
    public DateOnly DateString { get; set; }
    public decimal Cost { get; set; }
}