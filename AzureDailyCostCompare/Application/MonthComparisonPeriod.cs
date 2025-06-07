namespace AzureDailyCostCompare.Application;

public class MonthComparisonPeriod
{
    public DateTime CurrentFirstDayOfMonth { get; init; }
    public DateTime PreviousFirstDayOfMonth { get; init; }
    public int CurrentMonthDaysCount { get; init; }
    public int PreviousMonthDaysCount { get; init; }
}