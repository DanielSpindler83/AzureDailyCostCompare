namespace AzureDailyCostCompare.Application;

public class MonthCalculationService
{
    public (DateTime CurrentStart, DateTime PreviousStart, int CurrentDays, int PreviousDays)
        CalculateMonthBoundaries(DateTime referenceDate)
    {
        var currentStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var previousStart = currentStart.AddMonths(-1);
        var currentDays = DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
        var previousDays = DateTime.DaysInMonth(previousStart.Year, previousStart.Month);

        return (currentStart, previousStart, currentDays, previousDays);
    }
}