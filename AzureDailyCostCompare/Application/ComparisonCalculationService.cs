using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class ComparisonCalculationService
{
    private DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    public int CalculateComparisonDayCount(DateTime validatedDate)
    {
        if (DateIsInCompleteMonthInThePast(validatedDate))
        {
            int overrideMonthDays = DateTime.DaysInMonth(validatedDate.Year, validatedDate.Month);
            DateTime previousMonth = validatedDate.AddMonths(-1);
            int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            return Math.Max(overrideMonthDays, previousMonthDays); // longer of the two months is the day count
        }

        return validatedDate.Day; // we are in the current month so just use the day as the count
    }

    public DateTime AdjustForHistoricalDate(DateTime validatedDate)
    {
        if (DateIsInCompleteMonthInThePast(validatedDate))
        {
            var countOfDaysInMonth = DateTime.DaysInMonth(validatedDate.Year, validatedDate.Month);
            return new DateTime(validatedDate.Year, validatedDate.Month, countOfDaysInMonth);
        }
        return validatedDate;
    }

    private bool DateIsInCompleteMonthInThePast(DateTime validatedDate)
    {
        DateTime startOfCurrentMonth = new DateTime(CurrentDateTimeUtc.Year, CurrentDateTimeUtc.Month, 1);
        bool isCompleteMonthInPast = validatedDate < startOfCurrentMonth;
        return isCompleteMonthInPast;
    }
}
