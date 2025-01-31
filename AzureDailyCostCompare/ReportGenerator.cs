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

        averageCurrentPartialMonth = currentMonthCostData
            .Take(currentMonthCostData.Count)
            .Average(dc => dc.Cost);

        averagePreviousPartialMonth = previousMonthCostData
            .Take(currentMonthCostData.Count)
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
        return from day in Enumerable.Range(1, dateHelperService.OutputTableDaysToDisplay)
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

        PrintSectionHeader("Data Reference Information");
        Console.WriteLine($"All costs in USD");
        Console.WriteLine($"A day's data is considered complete {DateHelperService.FULL_DAY_DATA_CUTOFF_HOUR_UTC} hours after the end of the day in UTC time.");
        PrintDataReferenceDetails(dateHelperService.DataReferenceDate, localDataReferenceDay, localTimeZone);
    }

    private void PrintMonthlyAveragesTable()
    {
        // Table header
        Console.WriteLine("\n{0,-70} {1,10}", "Monthly Cost Averages", "Amount (USD)");
        Console.WriteLine(new string('-', 82));

        // Current month partial average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{dateHelperService.DataReferenceDate:MMMM} average (for {currentMonthCostData.Count} days)",
            averageCurrentPartialMonth);

        // Previous month partial average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{dateHelperService.DataReferenceDate.AddMonths(-1):MMMM} average (for {currentMonthCostData.Count} days)",
            averagePreviousPartialMonth);

        // Cost delta
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"Month averages cost delta ({dateHelperService.DataReferenceDate:MMMM} average minus {dateHelperService.DataReferenceDate.AddMonths(-1):MMMM} average)",
            currentToPreviousMonthAveragesCostDelta);

        // Previous full month average
        Console.WriteLine("{0,-70} {1,10:F2}",
            $"{dateHelperService.DataReferenceDate.AddMonths(-1):MMMM} full month average",
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