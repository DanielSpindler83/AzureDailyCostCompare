using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Infrastructure;

public class ConsoleReportRenderer : IReportRenderer
{
    private readonly DailyCostTableBuilder _dailyCostTableBuilder;
    private readonly WeeklyAnalysisService _weeklyAnalysisService;

    public ConsoleReportRenderer(
        DailyCostTableBuilder dailyCostTableBuilder,
        WeeklyAnalysisService weeklyAnalysisService)
    {
        _dailyCostTableBuilder = dailyCostTableBuilder;
        _weeklyAnalysisService = weeklyAnalysisService;
    }

    public void RenderDailyCostTable(ProcessedCostData data, CostComparisonContext context)
    {
        var tableRows = _dailyCostTableBuilder.BuildDailyCostTableData(data, context);

        Console.WriteLine("\n{0,-18} {1,-18} {2,-18} {3,-18}",
            "Day of Month",
            context.PreviousMonthStart.ToString("MMMM"),
            context.CurrentMonthStart.ToString("MMMM"),
            "Cost Difference(USD)");

        foreach (var row in tableRows)
        {
            string previousCost = FormatCost(row.PreviousCost);
            string currentCost = FormatCost(row.CurrentCost);
            string costDifference = row.CostDifference.ToString("F2");

            Console.WriteLine("{0,-18} {1,-18} {2,-18} {3,-18}",
                row.DayOfMonth,
                previousCost,
                currentCost,
                costDifference);
        }
    }

    public void RenderWeeklyComparisons(ProcessedCostData data, CostComparisonContext context)
    {
        Console.WriteLine("\n------ Weekly Pattern Analysis (UTC) ------");
        Console.WriteLine("Comparing corresponding weeks (1st Monday to 1st Monday, etc.)\n");

        var comparisons = _weeklyAnalysisService.GetWeeklyComparisons(data, context);

        foreach (var dayGroup in comparisons.GroupBy(c => c.DayOfWeek).OrderBy(g => g.Key))
        {
            Console.WriteLine($"{dayGroup.Key}s:");
            foreach (var comp in dayGroup.OrderBy(c => c.WeekNumber))
            {
                string weekLabel = GetWeekLabel(comp.WeekNumber);
                string costDiff = comp.CostDifference.ToString("+0.00;-0.00;0.00");

                Console.WriteLine(
                    $"{weekLabel,-8} {comp.PreviousDate:MMM dd} (UTC):{comp.PreviousCost,10:F2} -> " +
                    $"{comp.CurrentDate:MMM dd} (UTC):{comp.CurrentCost,10:F2} = {costDiff,10}");
            }
            Console.WriteLine();
        }
    }

    public void RenderDayOfWeekAverages(ProcessedCostData data, CostComparisonContext context)
    {
        Console.WriteLine("\n------ Day of Week Averages (UTC) ------");
        var averages = _weeklyAnalysisService.CalculateDayOfWeekAverages(data, context);

        if (averages.Count == 0)
        {
            Console.WriteLine("No complete day-of-week data available for comparison yet.");
            return;
        }

        Console.WriteLine($"{"Day",-10} {"Prev Avg",12} {"Curr Avg",12} {"Diff",12} {"Samples",8}");
        Console.WriteLine(new string('-', 55));

        foreach (var avg in averages.OrderBy(a => a.DayOfWeek))
        {
            string diff = avg.Difference.ToString("+0.00;-0.00;0.00");
            string samples = $"({avg.PreviousMonthCount}/{avg.CurrentMonthCount})";

            Console.WriteLine(
                $"{avg.DayOfWeek,-10} {avg.PreviousMonthAverage,12:F2} " +
                $"{avg.CurrentMonthAverage,12:F2} {diff,12} {samples,8}");
        }
    }

    public void RenderDataAnalysisAndInfo(ProcessedCostData data, CostComparisonContext context)
    {
        RenderMonthlyAveragesTable(data, context);
        RenderDataReferenceInfo(context);
    }

    private void RenderMonthlyAveragesTable(ProcessedCostData data, CostComparisonContext context)
    {
        Console.WriteLine("\n{0,-70} {1,10}", "Monthly Cost Averages", "Amount (USD)");
        Console.WriteLine(new string('-', 82));

        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{context.ComparisonReferenceDate:MMMM} average (for {data.LikeForLikeDayCount} days)",
            data.CurrentMonthLikeForLikeAverage);

        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{context.ComparisonReferenceDate.AddMonths(-1):MMMM} average (for {data.LikeForLikeDayCount} days)",
            data.PreviousMonthLikeForLikeAverage);

        Console.WriteLine("{0,-70} {1,10:F2}",
            $"Month averages same day count cost delta ({context.ComparisonReferenceDate:MMMM} avg - {context.ComparisonReferenceDate.AddMonths(-1):MMMM} avg)",
            data.LikeForLikeDailyAverageDelta);

        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{context.ComparisonReferenceDate.AddMonths(-1):MMMM} full month average",
            data.PreviousMonthFullAverage);

        if (data.CurrentMonthExtraDaysAverage is not null)
        {
            Console.WriteLine("{0,-70} {1,10:F2}",
                $"{context.ComparisonReferenceDate:MMMM} average (for {context.ComparisonTableDayCount} days)",
                data.CurrentMonthExtraDaysAverage);
        }
    }

    private void RenderDataReferenceInfo(CostComparisonContext context)
    {
        Console.WriteLine($"\n------ Data Reference Information ------");

        var localTimeZone = TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(context.ComparisonReferenceDate, localTimeZone);
        //dataCompleteDateTime = 

        Console.WriteLine($"All costs in USD");
        Console.WriteLine($"A day's data is considered complete {context.DataLoadDelayHours} hours after the end of the day in UTC time.");
        Console.WriteLine($"Daily cost data is complete up to end of the day {context.ComparisonReferenceDate:dd/MM/yyyy} in UTC timezone"); // BUG HERE the date value is WRONG - review
        Console.WriteLine($"The end of the day in UTC time is {localDataReferenceDay} in local timezone of {localTimeZone.DisplayName}");
        Console.WriteLine($"\nThis report was generated at {DateTime.Now} {localTimeZone.DisplayName}");
        Console.WriteLine($"This report was generated at {DateTime.UtcNow} UTC\n");
    }

    private static string FormatCost(decimal? cost) => cost.HasValue ? cost.Value.ToString("F2") : "";

    private static string GetWeekLabel(int weekNumber) => weekNumber switch
    {
        1 => "First",
        2 => "Second",
        3 => "Third",
        4 => "Fourth",
        5 => "Fifth",
        _ => $"Week {weekNumber}"
    };
}