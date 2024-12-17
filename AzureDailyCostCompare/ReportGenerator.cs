namespace AzureDailyCostCompare;

class ReportGenerator
{
    private readonly DateHelperService dateHelperService;
    private readonly List<DailyCostData> currentMonthCostData;
    private readonly List<DailyCostData> previousMonthCostData;
    private readonly decimal averageCurrentPartialMonth;
    private readonly decimal averagePreviousPartialMonth;
    private readonly decimal averagePreviousFullMonth;
    private readonly decimal currentToPreviousMonthAveragesCostDelta;

    public ReportGenerator(List<DailyCostData> costData, DateHelperService dateHelperService)
    {

        this.dateHelperService = dateHelperService;

        currentMonthCostData = costData
                .Where(dc => dc.DateString.Month == dateHelperService.FirstDayOfCurrentMonth.Month && dc.DateString.Year == dateHelperService.FirstDayOfCurrentMonth.Year)
                .ToList();
        previousMonthCostData = costData
                .Where(dc => dc.DateString.Month == dateHelperService.FirstDayOfPreviousMonth.Month && dc.DateString.Year == dateHelperService.FirstDayOfPreviousMonth.Year)
                .ToList();

        var dayCountCurrentMonth = currentMonthCostData.Count;

        averageCurrentPartialMonth = currentMonthCostData
            .Take(dayCountCurrentMonth)
            .Average(dc => dc.Cost);

        averagePreviousPartialMonth = previousMonthCostData
            .Take(dayCountCurrentMonth)
            .Average(dc => dc.Cost);

        currentToPreviousMonthAveragesCostDelta = averageCurrentPartialMonth - averagePreviousPartialMonth;

        averagePreviousFullMonth = previousMonthCostData.Average(dc => dc.Cost);

    }

    public void GenerateDailyCostReport()
    {
        var tableData = CreateDailyCostTableData();
        PrintDailyCostTable(tableData);
        PrintDataAnalysisAndInfo();
    }

    private IEnumerable<DailyCostRow> CreateDailyCostTableData()
    {
        return from day in Enumerable.Range(1, GetMaxDaysAcrossMonths())
               let currentDay = FindDayInCurrentMonth(day)
               let previousDay = FindDayInPreviousMonth(day)
               select new DailyCostRow
               {
                   DayOfMonth = day,
                   PreviousCost = DetermineMonthCost(previousDay, day, dateHelperService.CountOfDaysInPreviousMonth),
                   CurrentCost = DetermineMonthCost(currentDay, day, dateHelperService.CountOfDaysInCurrentMonth),
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
            dateHelperService.FirstDayOfPreviousMonth.ToString("MMMM"),
            dateHelperService.FirstDayOfCurrentMonth.ToString("MMMM"),
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

    private int GetMaxDaysAcrossMonths() => Math.Max(dateHelperService.CountOfDaysInPreviousMonth, dateHelperService.CountOfDaysInCurrentMonth);

    private DailyCostData? FindDayInCurrentMonth(int day) => currentMonthCostData.FirstOrDefault(d => d.DateString.Day == day);

    private DailyCostData? FindDayInPreviousMonth(int day) => previousMonthCostData.FirstOrDefault(d => d.DateString.Day == day); 

    private static decimal? DetermineMonthCost(DailyCostData dayData, int day, int monthDaysCount) => dayData?.Cost ?? (day > monthDaysCount ? (decimal?)null : 0);

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
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(dateHelperService.DataReferenceDate, localTimeZone);

        PrintMonthlyAveragesTable();

        PrintSectionHeader("Cost Analysis");
        Console.WriteLine($"All costs in USD");
        Console.WriteLine($"A day's data is considered complete {DateHelperService.FULL_DAY_DATA_CUTOFF_HOUR_UTC} hours after the end of the day in UTC time.");

        PrintSectionHeader("Data Reference Information");
        PrintDataReferenceDetails(dateHelperService.DataReferenceDate, localDataReferenceDay, localTimeZone);
    }

    private void PrintMonthlyAveragesTable()
    {
        PrintSectionHeader("Monthly Cost Averages");

        // Table header
        Console.WriteLine("{0,-60} {1,10}", "Metric", "Amount (USD)");
        Console.WriteLine(new string('-', 72));

        // Current month partial average
        Console.WriteLine("{0,-60} {1,10:F2}",
            $"Current month average (for {currentMonthCostData.Count} days)",
            averageCurrentPartialMonth);

        // Previous month partial average
        Console.WriteLine("{0,-60} {1,10:F2}",
            $"Previous month average (for {currentMonthCostData.Count} days)",
            averagePreviousPartialMonth);

        // Cost delta
        Console.WriteLine("{0,-60} {1,10:F2}",
            "Current to previous month averages cost delta",
            currentToPreviousMonthAveragesCostDelta);

        // Previous full month average
        Console.WriteLine("{0,-60} {1,10:F2}",
            "Previous Full Month Average",
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
}