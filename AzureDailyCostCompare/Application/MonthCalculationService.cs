namespace AzureDailyCostCompare.Application;

public class MonthCalculationService
{
    public MonthComparisonPeriod CalculateMonthComparisonPeriod(DateTime validatedComparisonDate)
    {
        var currentStart = new DateTime(validatedComparisonDate.Year, validatedComparisonDate.Month, 1);
        var previousStart = currentStart.AddMonths(-1);
        var currentDays = DateTime.DaysInMonth(validatedComparisonDate.Year, validatedComparisonDate.Month);
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