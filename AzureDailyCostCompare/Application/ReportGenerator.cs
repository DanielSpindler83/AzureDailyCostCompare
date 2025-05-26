namespace AzureDailyCostCompare.Application;

public class ReportGenerator
{
    private readonly CostComparisonDateService costComparisonDateService;
    private readonly List<DailyCostData> currentMonthCostData;
    private readonly List<DailyCostData> previousMonthCostData;
    private readonly decimal averageCurrentPartialMonth;
    private readonly decimal averagePreviousPartialMonth;
    private readonly decimal averagePreviousFullMonth;
    private readonly decimal currentToPreviousMonthAveragesCostDelta;

    public ReportGenerator(List<DailyCostData> costData, CostComparisonDateService costComparisonDateService)
    {

        this.costComparisonDateService = costComparisonDateService;

        currentMonthCostData = costData
                .Where(dc => dc.DateString.Month == costComparisonDateService.CurrentMonthStartDate.Month && dc.DateString.Year == costComparisonDateService.CurrentMonthStartDate.Year)
                .ToList();

        previousMonthCostData = costData
                .Where(dc => dc.DateString.Month == costComparisonDateService.PreviousMonthStartDate.Month && dc.DateString.Year == costComparisonDateService.PreviousMonthStartDate.Year)
                .ToList();

        averageCurrentPartialMonth = currentMonthCostData
            .Take(currentMonthCostData.Count)
            .Average(dc => dc.Cost);

        averagePreviousPartialMonth = previousMonthCostData
            .Take(currentMonthCostData.Count)
            .Average(dc => dc.Cost);

        currentToPreviousMonthAveragesCostDelta = averageCurrentPartialMonth - averagePreviousPartialMonth;

        averagePreviousFullMonth = previousMonthCostData.Average(dc => dc.Cost);

    }

    public void GenerateDailyCostReport(bool showWeeklyPatterns, bool showDayOfWeekAverages)
    {
        var tableData = CreateDailyCostTableData();
        PrintDailyCostTable(tableData);
        if (showWeeklyPatterns)
            PrintWeeklyComparisons();
        if (showDayOfWeekAverages)
            PrintDayOfWeekAverages();
        PrintDataAnalysisAndInfo();
    }

    private IEnumerable<DailyCostRow> CreateDailyCostTableData()
    {
        return from day in Enumerable.Range(1, costComparisonDateService.ComparisonTableDayCount)
               let currentDay = FindDayInCurrentMonth(day)
               let previousDay = FindDayInPreviousMonth(day)
               select new DailyCostRow
               {
                   DayOfMonth = day,
                   PreviousCost = DetermineMonthCost(previousDay, day, costComparisonDateService.PreviousMonthDayCount),
                   CurrentCost = DetermineMonthCost(currentDay, day, costComparisonDateService.CurrentMonthDayCount),
                   CostDifference = CalculateCostDifference(currentDay, previousDay)
               };
    }

    private void PrintDailyCostTable(IEnumerable<DailyCostRow> tableData)
    {
        PrintTableHeader();
        foreach (var row in tableData)
        {
            PrintTableRow(row);
        }
    }

    private void PrintTableHeader()
    {
        Console.WriteLine("\n{0,-18} {1,-18} {2,-18} {3,-18}",
            "Day of Month",
            costComparisonDateService.PreviousMonthStartDate.ToString("MMMM"),
            costComparisonDateService.CurrentMonthStartDate.ToString("MMMM"),
            "Cost Difference(USD)");
    }

    private static void PrintTableRow(DailyCostRow row)
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

    private static string FormatCost(decimal? cost) => cost.HasValue ? cost.Value.ToString("F2") : "";

    private DailyCostData? FindDayInCurrentMonth(int day) => currentMonthCostData.FirstOrDefault(d => d.DateString.Day == day);

    private DailyCostData? FindDayInPreviousMonth(int day) => previousMonthCostData.FirstOrDefault(d => d.DateString.Day == day);

    private static decimal? DetermineMonthCost(DailyCostData dayData, int day, int monthDaysCount) => dayData?.Cost ?? (day > monthDaysCount ? null : 0);

    private static decimal CalculateCostDifference(DailyCostData currentDay, DailyCostData previousDay) => (currentDay?.Cost ?? 0) - (previousDay?.Cost ?? 0);

    private class DailyCostRow
    {
        public int DayOfMonth { get; set; }
        public decimal? PreviousCost { get; set; }
        public decimal? CurrentCost { get; set; }
        public decimal CostDifference { get; set; }
    }

    private void PrintDataAnalysisAndInfo()
    {
        var localTimeZone = TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(costComparisonDateService.CostDataReferenceDate, localTimeZone);

        PrintMonthlyAveragesTable();

        PrintSectionHeader("Data Reference Information");
        Console.WriteLine($"All costs in USD");
        Console.WriteLine($"A day's data is considered complete {costComparisonDateService.DataAvailabilityCutoffHourUtc} hours after the end of the day in UTC time.");
        PrintDataReferenceDetails(costComparisonDateService.CostDataReferenceDate, localDataReferenceDay, localTimeZone);
    }

    private void PrintMonthlyAveragesTable()
    {
        // Table header
        Console.WriteLine("\n{0,-70} {1,10}", "Monthly Cost Averages", "Amount (USD)");
        Console.WriteLine(new string('-', 82));

        // Current month partial average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{costComparisonDateService.CostDataReferenceDate:MMMM} average (for {currentMonthCostData.Count} days)",
            averageCurrentPartialMonth);

        // Previous month partial average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{costComparisonDateService.CostDataReferenceDate.AddMonths(-1):MMMM} average (for {currentMonthCostData.Count} days)",
            averagePreviousPartialMonth);

        // Cost delta
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"Month averages cost delta ({costComparisonDateService.CostDataReferenceDate:MMMM} average minus {costComparisonDateService.CostDataReferenceDate.AddMonths(-1):MMMM} average)",
            currentToPreviousMonthAveragesCostDelta);

        // Previous full month average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{costComparisonDateService.CostDataReferenceDate.AddMonths(-1):MMMM} full month average",
            averagePreviousFullMonth);
    }

    private static void PrintSectionHeader(string headerText)
    {
        Console.WriteLine($"\n------ {headerText} ------");
    }

    private static void PrintDataReferenceDetails(DateTime dataReferenceDate, DateTime localDataReferenceDay, TimeZoneInfo localTimeZone)
    {
        Console.WriteLine($"Daily cost data is complete up to end of the day {dataReferenceDate:yyyy-MM-dd} in UTC timezone");
        Console.WriteLine($"The end of the day in UTC time is {localDataReferenceDay} in local timezone of {localTimeZone.DisplayName}");

        Console.WriteLine($"\nThis report was generated at {DateTime.Now} {localTimeZone.DisplayName}");
        Console.WriteLine($"This report was generated at {DateTime.UtcNow} UTC\n");
    }


    private class WeeklyComparison
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int WeekNumber { get; set; }
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public decimal? PreviousCost { get; set; }
        public decimal? CurrentCost { get; set; }
        public decimal CostDifference => (CurrentCost ?? 0) - (PreviousCost ?? 0);
    }

    private class DayOfWeekAverage
    {
        public DayOfWeek DayOfWeek { get; set; }
        public decimal PreviousMonthAverage { get; set; }
        public decimal CurrentMonthAverage { get; set; }
        public decimal Difference => CurrentMonthAverage - PreviousMonthAverage;
        public int PreviousMonthCount { get; set; }
        public int CurrentMonthCount { get; set; }
    }

    private void PrintWeeklyComparisons()
    {
        Console.WriteLine("\n------ Weekly Pattern Analysis (UTC) ------");
        Console.WriteLine("Comparing corresponding weeks (1st Monday to 1st Monday, etc.)\n");

        var comparisons = GetWeeklyComparisons();
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

    private void PrintDayOfWeekAverages()
    {
        Console.WriteLine("\n------ Day of Week Averages (UTC) ------");
        var averages = CalculateDayOfWeekAverages();

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

        //Console.WriteLine($"\nData complete through: {costComparisonDateService.DataReferenceDate:yyyy-MM-dd HH:mm} UTC");
    }

    private List<WeeklyComparison> GetWeeklyComparisons()
    {
        var comparisons = new List<WeeklyComparison>();
        var previousMonth = costComparisonDateService.PreviousMonthStartDate; // Already UTC
        var currentMonth = costComparisonDateService.CurrentMonthStartDate;   // Already UTC

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var previousWeeks = GetWeeksForDay(day, previousMonth)
                .Where(d => d <= costComparisonDateService.CostDataReferenceDate)
                .ToList();

            var currentWeeks = GetWeeksForDay(day, currentMonth)
                .Where(d => d <= costComparisonDateService.CostDataReferenceDate)
                .ToList();

            for (int weekNum = 0; weekNum < Math.Min(previousWeeks.Count, currentWeeks.Count); weekNum++)
            {
                var prevDate = previousWeeks[weekNum];
                var currDate = currentWeeks[weekNum];

                var prevCost = FindDayInPreviousMonth(prevDate.Day)?.Cost;
                var currCost = FindDayInCurrentMonth(currDate.Day)?.Cost;

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

    private List<DayOfWeekAverage> CalculateDayOfWeekAverages()
    {
        var averages = new List<DayOfWeekAverage>();

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            // Use UTC DateString for day-of-week calculation
            var previousDays = previousMonthCostData
                .Where(d => d.DateString.DayOfWeek == day &&
                           d.DateString <= DateOnly.FromDateTime(costComparisonDateService.CostDataReferenceDate))
                .ToList();

            var currentDays = currentMonthCostData
                .Where(d => d.DateString.DayOfWeek == day &&
                           d.DateString <= DateOnly.FromDateTime(costComparisonDateService.CostDataReferenceDate))
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
        // Ensure we're creating UTC dates
        var current = new DateTime(monthUtc.Year, monthUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDay = new DateTime(monthUtc.Year, monthUtc.Month,
            DateTime.DaysInMonth(monthUtc.Year, monthUtc.Month), 23, 59, 59, DateTimeKind.Utc);

        // Find first occurrence
        while (current.DayOfWeek != targetDay && current <= lastDay)
        {
            current = current.AddDays(1);
        }

        // Add all occurrences
        while (current <= lastDay)
        {
            dates.Add(current);
            current = current.AddDays(7);
        }

        return dates;
    }

    private static string GetWeekLabel(int weekNumber)
    {
        return weekNumber switch
        {
            1 => "First",
            2 => "Second",
            3 => "Third",
            4 => "Fourth",
            5 => "Fifth",
            _ => $"Week {weekNumber}"
        };
    }


}