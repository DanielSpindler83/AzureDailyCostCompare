namespace AzureDailyCostCompare;

record DailyCostData
{
    public DateOnly DateString { get; set; }
    public decimal Cost { get; set; }
}