using AzureDailyCostCompare.Application;
using AzureDailyCostCompare.Domain;
using Spectre.Console;
using System.Data;
using Rule = Spectre.Console.Rule;

namespace AzureDailyCostCompare.Infrastructure;

public class SpectreReportRenderer(
    DailyCostTableBuilder dailyCostTableBuilder,
    WeeklyAnalysisService weeklyAnalysisService) : IReportRenderer
{
    private readonly DailyCostTableBuilder _dailyCostTableBuilder = dailyCostTableBuilder;
    private readonly WeeklyAnalysisService _weeklyAnalysisService = weeklyAnalysisService;

    public void RenderDailyCostTable(ProcessedCostData data, CostComparisonContext context)
    {
        var tableRows = _dailyCostTableBuilder.BuildDailyCostTableData(data, context);

        var table = new Table()
            .AddColumn("Day of Month")
            .AddColumn(context.PreviousMonthStart.ToString("MMMM"))
            .AddColumn(context.CurrentMonthStart.ToString("MMMM"))
            .AddColumn("Cost Difference (USD)");

        foreach (var row in tableRows)
        {
            table.AddRow(
                row.DayOfMonth.ToString(),
                FormatCost(row.PreviousCost),
                FormatCost(row.CurrentCost),
                row.CostDifference.ToString("F2")
            );
        }
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
    }

    public void RenderWeeklyComparisons(ProcessedCostData data, CostComparisonContext context)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Weekly Pattern Analysis (UTC)[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Comparing corresponding weeks (1st Monday to 1st Monday, etc.)\n");
        AnsiConsole.WriteLine();

        var comparisons = _weeklyAnalysisService.GetWeeklyComparisons(data, context);

        foreach (var dayGroup in comparisons.GroupBy(c => c.DayOfWeek).OrderBy(g => g.Key))
        {
            AnsiConsole.Write(new Markup($"[bold cyan]{dayGroup.Key}s:[/]\n"));

            var weekTable = new Table()
                .Border(TableBorder.None)
                .AddColumn("Week")
                .AddColumn("Previous")
                .AddColumn("Current")
                .AddColumn("Difference");

            foreach (var comp in dayGroup.OrderBy(c => c.WeekNumber))
            {
                string weekLabel = GetWeekLabel(comp.WeekNumber);
                string costDiff = comp.CostDifference.ToString("+0.00;-0.00;0.00");
                var diffColor = comp.CostDifference > 0 ? "red" : comp.CostDifference < 0 ? "green" : "white";

                weekTable.AddRow(
                    weekLabel,
                    $"{comp.PreviousDate:MMM dd}: {comp.PreviousCost:F2}",
                    $"{comp.CurrentDate:MMM dd}: {comp.CurrentCost:F2}",
                    $"[{diffColor}]{costDiff}[/]"
                );
            }

            AnsiConsole.Write(weekTable);
            AnsiConsole.WriteLine();
        }
    }

    public void RenderDayOfWeekAverages(ProcessedCostData data, CostComparisonContext context)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Day of Week Averages (UTC)[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();

        var averages = _weeklyAnalysisService.CalculateDayOfWeekAverages(data, context);

        if (averages.Count == 0)
        {
            AnsiConsole.WriteLine("No complete day-of-week data available for comparison yet.");
            return;
        }

        var table = new Table()
            .AddColumn("Day")
            .AddColumn("Previous Avg")
            .AddColumn("Current Avg")
            .AddColumn("Difference")
            .AddColumn("Samples");

        foreach (var avg in averages.OrderBy(a => a.DayOfWeek))
        {
            string diff = avg.Difference.ToString("+0.00;-0.00;0.00");
            string samples = $"({avg.PreviousMonthCount}/{avg.CurrentMonthCount})";
            var diffColor = avg.Difference > 0 ? "red" : avg.Difference < 0 ? "green" : "white";

            table.AddRow(
                avg.DayOfWeek.ToString(),
                avg.PreviousMonthAverage.ToString("F2"),
                avg.CurrentMonthAverage.ToString("F2"),
                $"[{diffColor}]{diff}[/]",
                samples
            );
        }

        AnsiConsole.Write(table);
    }

    public void RenderDataAnalysisAndInfo(ProcessedCostData data, CostComparisonContext context)
    {
        RenderMonthlyAveragesTable(data, context);
        RenderDataReferenceInfo(context);
    }

    private static void RenderMonthlyAveragesTable(ProcessedCostData data, CostComparisonContext context)
    {
        var comparison = data.MonthComparison;

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Monthly Cost Averages[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();

        if (comparison.IsUnevenComparison)
        {
            // Show split sections when day counts differ
            RenderLikeForLikeComparison(data, context);
            RenderFullMonthComparison(data, context);
        }
        else
        {
            // Show original single section when day counts are the same
            RenderStandardComparison(data, context);
        }
    }

    private static void RenderStandardComparison(ProcessedCostData data, CostComparisonContext context)
    {
        var comparison = data.MonthComparison;

        var table = new Table()
            .AddColumn("Description")
            .AddColumn(new TableColumn("Amount (USD)").RightAligned());

        table.AddRow(
            $"{context.ReferenceDate:MMMM} average (for {comparison.CurrentDayCount} days)",
            comparison.CurrentFullAverage.ToString("F2"));

        table.AddRow(
            $"{context.ReferenceDate.AddMonths(-1):MMMM} average (for {comparison.LikeForLikeDays} days)",
            comparison.PreviousLikeForLikeAverage.ToString("F2"));

        var deltaColor = comparison.FullComparisonDelta > 0 ? "red" :
                        comparison.FullComparisonDelta < 0 ? "green" : "white";

        table.AddRow(
            $"Month averages cost delta ({context.ReferenceDate:MMMM} minus {context.ReferenceDate.AddMonths(-1):MMMM})",
            $"[{deltaColor}]{comparison.FullComparisonDelta:F2}[/]");

        table.AddRow(
            $"{context.ReferenceDate.AddMonths(-1):MMMM} full month average ({comparison.PreviousMonthTotalDays} days)",
            comparison.PreviousFullAverage.ToString("F2"));

        AnsiConsole.Write(table);
    }

    private static void RenderLikeForLikeComparison(ProcessedCostData data, CostComparisonContext context)
    {
        var comparison = data.MonthComparison;

        AnsiConsole.Write(new Markup("[bold cyan]Comparison for Same Number of Days[/]\n"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .AddColumn("Description")
            .AddColumn(new TableColumn("Amount (USD)").RightAligned());

        table.AddRow(
            $"{context.ReferenceDate:MMMM} average ({comparison.LikeForLikeDays} days)",
            comparison.CurrentLikeForLikeAverage.ToString("F2"));

        table.AddRow(
            $"{context.ReferenceDate.AddMonths(-1):MMMM} average ({comparison.LikeForLikeDays} days)",
            comparison.PreviousLikeForLikeAverage.ToString("F2"));

        var deltaColor = comparison.LikeForLikeDelta > 0 ? "red" :
                        comparison.LikeForLikeDelta < 0 ? "green" : "white";

        table.AddRow(
            $"Cost delta ({context.ReferenceDate:MMMM} minus {context.ReferenceDate.AddMonths(-1):MMMM} - both same number of days.)",
            $"[{deltaColor}]{comparison.LikeForLikeDelta:F2}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static void RenderFullMonthComparison(ProcessedCostData data, CostComparisonContext context)
    {
        var comparison = data.MonthComparison;

        AnsiConsole.Write(new Markup("[bold cyan]Running Average[/]\n"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .AddColumn("Description")
            .AddColumn(new TableColumn("Amount (USD)").RightAligned());

        table.AddRow(
            $"{context.ReferenceDate:MMMM} average ({comparison.CurrentDayCount} days)",
            comparison.CurrentFullAverage.ToString("F2"));

        table.AddRow(
            $"{context.ReferenceDate.AddMonths(-1):MMMM} full month average ({comparison.PreviousMonthTotalDays} days)",
            comparison.PreviousFullAverage.ToString("F2"));

        var deltaColor = comparison.FullComparisonDelta > 0 ? "red" :
                        comparison.FullComparisonDelta < 0 ? "green" : "white";

        table.AddRow(
            $"Cost delta ({context.ReferenceDate:MMMM} minus {context.ReferenceDate.AddMonths(-1):MMMM})",
            $"[{deltaColor}]{comparison.FullComparisonDelta:F2}[/]");

        AnsiConsole.Write(table);

        // Add note for uneven comparison
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Markup($"[italic yellow]Note: Comparing {comparison.CurrentDayCount} current days vs {comparison.PreviousMonthTotalDays}-day {context.ReferenceDate.AddMonths(-1):MMMM} total[/]"));
    }

    private static void RenderDataReferenceInfo(CostComparisonContext context)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Data Reference Information[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();

        var localTimeZone = TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(context.ReferenceDate, localTimeZone);

        var panel = new Panel(new Markup(
            $"[bold]All costs in USD[/]\n\n" +
            $"A day's data is considered complete [cyan]{context.DataLoadDelayHours} hours[/] after the end of the day in UTC time.\n\n" +
            $"Daily cost data is complete up to end of the day [cyan]{context.ReferenceDate}[/] in UTC timezone\n" +
            $"The end of the day in UTC time is [cyan]{localDataReferenceDay}[/] in local timezone of [cyan]{localTimeZone.DisplayName}[/]\n\n" +
            $"This report was generated at [cyan]{DateTime.Now}[/] {localTimeZone.DisplayName}\n" +
            $"This report was generated at [cyan]{DateTime.UtcNow}[/] UTC"))
            .Header("Data Reference");

        AnsiConsole.Write(panel);
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