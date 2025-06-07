namespace AzureDailyCostCompare.Application;

public class MonthCalculationService
{
    public MonthComparisonPeriod CalculateMonthComparisonPeriod(DateTime referenceDate)
    {
        var currentStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var previousStart = currentStart.AddMonths(-1);
        var currentDays = DateTime.DaysInMonth(referenceDate.Year, referenceDate.Month);
        var previousDays = DateTime.DaysInMonth(previousStart.Year, previousStart.Month);

        return new MonthComparisonPeriod
        {
            CurrentFirstDayOfMonth = currentStart,
            PreviousFirstDayOfMonth = previousStart,
            CurrentMonthDaysCount = currentDays,
            PreviousMonthDaysCount = previousDays
        };
    }
}