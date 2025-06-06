using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Application;

public class WeeklyAnalysisService
{
    public List<WeeklyComparison> GetWeeklyComparisons(ProcessedCostData data, CostComparisonContext context)
    {
        var comparisons = new List<WeeklyComparison>();

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var previousWeeks = GetWeeksForDay(day, context.PreviousMonthStart)
                .Where(d => d <= context.ComparisonReferenceDate)
                .ToList();

            var currentWeeks = GetWeeksForDay(day, context.CurrentMonthStart)
                .Where(d => d <= context.ComparisonReferenceDate)
                .ToList();

            for (int weekNum = 0; weekNum < Math.Min(previousWeeks.Count, currentWeeks.Count); weekNum++)
            {
                var prevDate = previousWeeks[weekNum];
                var currDate = currentWeeks[weekNum];

                var prevCost = data.PreviousMonthDailyCosts.FirstOrDefault(d => d.DateString.Day == prevDate.Day)?.Cost;
                var currCost = data.CurrentMonthDailyCosts.FirstOrDefault(d => d.DateString.Day == currDate.Day)?.Cost;

                if (prevCost.HasValue && currCost.HasValue)
                {
                    comparisons.Add(new WeeklyComparison
                    {
                        DayOfWeek = day,
                        WeekNumber = weekNum + 1,
                        PreviousDate = prevDate,
                        CurrentDate = currDate,
                        PreviousCost = prevCost,
                        CurrentCost = currCost
                    });
                }
            }
        }

        return comparisons;
    }

    public List<DayOfWeekAverage> CalculateDayOfWeekAverages(ProcessedCostData data, CostComparisonContext context)
    {
        var averages = new List<DayOfWeekAverage>();

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var previousDays = data.PreviousMonthDailyCosts
                .Where(d => d.DateString.DayOfWeek == day &&
                           d.DateString <= DateOnly.FromDateTime(context.ComparisonReferenceDate))
                .ToList();

            var currentDays = data.CurrentMonthDailyCosts
                .Where(d => d.DateString.DayOfWeek == day &&
                           d.DateString <= DateOnly.FromDateTime(context.ComparisonReferenceDate))
                .ToList();

            if (previousDays.Count != 0 || currentDays.Count != 0)
            {
                averages.Add(new DayOfWeekAverage
                {
                    DayOfWeek = day,
                    PreviousMonthAverage = previousDays.Count != 0 ? previousDays.Average(d => d.Cost) : 0,
                    CurrentMonthAverage = currentDays.Count != 0 ? currentDays.Average(d => d.Cost) : 0,
                    PreviousMonthCount = previousDays.Count,
                    CurrentMonthCount = currentDays.Count
                });
            }
        }

        return averages;
    }

    private static List<DateTime> GetWeeksForDay(DayOfWeek targetDay, DateTime monthUtc)
    {
        var dates = new List<DateTime>();
        var current = new DateTime(monthUtc.Year, monthUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDay = new DateTime(monthUtc.Year, monthUtc.Month,
            DateTime.DaysInMonth(monthUtc.Year, monthUtc.Month), 23, 59, 59, DateTimeKind.Utc);

        while (current.DayOfWeek != targetDay && current <= lastDay)
        {
            current = current.AddDays(1);
        }

        while (current <= lastDay)
        {
            dates.Add(current);
            current = current.AddDays(7);
        }

        return dates;
    }
}

public class WeeklyComparison
{
    public DayOfWeek DayOfWeek { get; set; }
    public int WeekNumber { get; set; }
    public DateTime PreviousDate { get; set; }
    public DateTime CurrentDate { get; set; }
    public decimal? PreviousCost { get; set; }
    public decimal? CurrentCost { get; set; }
    public decimal CostDifference => (CurrentCost ?? 0) - (PreviousCost ?? 0);
}

public class DayOfWeekAverage
{
    public DayOfWeek DayOfWeek { get; set; }
    public decimal PreviousMonthAverage { get; set; }
    public decimal CurrentMonthAverage { get; set; }
    public decimal Difference => CurrentMonthAverage - PreviousMonthAverage;
    public int PreviousMonthCount { get; set; }
    public int CurrentMonthCount { get; set; }
}