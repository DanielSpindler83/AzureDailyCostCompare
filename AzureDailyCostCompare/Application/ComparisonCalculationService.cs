using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class ComparisonCalculationService
{
    private DateTime CurrentDateTimeUtc { get; init; } = DateTime.UtcNow;

    public ComparisonType DetermineComparisonType(DateTime validatedDate)
    {
        return validatedDate.Month == CurrentDateTimeUtc.Month && validatedDate.Year == CurrentDateTimeUtc.Year
            ? ComparisonType.PartialMonth
            : ComparisonType.FullMonth;
    }

    public int CalculateComparisonDayCount(DateTime validatedDate)
    {
        var comparisonType = DetermineComparisonType(validatedDate);
        if (comparisonType == ComparisonType.PartialMonth)
        {
            return validatedDate.Day;
        }

        int overrideMonthDays = DateTime.DaysInMonth(validatedDate.Year, validatedDate.Month);
        DateTime previousMonth = validatedDate.AddMonths(-1);
        int previousMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        return Math.Max(overrideMonthDays, previousMonthDays);
    }

    public DateTime AdjustForHistoricalDate(DateTime validatedDate)
    {
        DateTime startOfCurrentMonth = new DateTime(CurrentDateTimeUtc.Year, CurrentDateTimeUtc.Month, 1);
        bool isCompleteMonthInPast = validatedDate < startOfCurrentMonth;

        if (isCompleteMonthInPast)
        {
            var countOfDaysInMonth = DateTime.DaysInMonth(validatedDate.Year, validatedDate.Month);
            return new DateTime(validatedDate.Year, validatedDate.Month, countOfDaysInMonth);
        }
        return validatedDate;
    }
}
