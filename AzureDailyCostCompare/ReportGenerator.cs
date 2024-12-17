namespace AzureDailyCostCompare;

class ReportGenerator
{
    private readonly DateHelperService dateHelperService;
    private readonly List<DailyCosts> currentMonthCostData;
    private readonly List<DailyCosts> previousMonthCostData;
    private readonly decimal averageCurrentPartialMonth;
    private readonly decimal averagePreviousPartialMonth;
    private readonly decimal averagePreviousFullMonth;
    private readonly decimal currentToPreviousMonthAveragesCostDelta;

    public ReportGenerator(List<DailyCosts> costData, DateHelperService dateHelperService)
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
        var tableData = from day in Enumerable.Range(1, Math.Max(dateHelperService.CountOfDaysInPreviousMonth, dateHelperService.CountOfDaysInCurrentMonth))
                        join currentDay in currentMonthCostData on day equals currentDay.DateString.Day into currentDays
                        from currentDay in currentDays.DefaultIfEmpty()
                        join previousDay in previousMonthCostData on day equals previousDay.DateString.Day into previousDays
                        from previousDay in previousDays.DefaultIfEmpty()
                        select new
                        {
                            DayOfMonth = day,
                            PreviousCost = previousDay?.Cost ?? (day > dateHelperService.CountOfDaysInPreviousMonth ? (decimal?)null : 0),
                            CurrentCost = currentDay?.Cost ?? (day > dateHelperService.CountOfDaysInCurrentMonth ? (decimal?)null : 0),
                            CostDifference = (currentDay?.Cost ?? 0) - (previousDay?.Cost ?? 0)
                        };

        // Print table header
        Console.WriteLine("{0,-18} {1,-18} {2,-18} {3,-18}", "Day of Month", dateHelperService.FirstDayOfPreviousMonth.ToString("MMMM"), dateHelperService.FirstDayOfCurrentMonth.ToString("MMMM"), "Cost Difference(USD)");

        // Print table data
        foreach (var row in tableData)
        {
            string previousCost = row.PreviousCost.HasValue ? row.PreviousCost.Value.ToString("F2") : "";
            string currentCost = row.CurrentCost.HasValue ? row.CurrentCost.Value.ToString("F2") : "";
            string costDifference = row.CostDifference.ToString("F2");

            Console.WriteLine("{0,-18} {1,-18} {2,-18} {3,-18}", row.DayOfMonth, previousCost, currentCost, costDifference);
        }

        PrintDataAnalysisAndInfo();
    }

    private void PrintDataAnalysisAndInfo()
    {
        var localTimeZone = TimeZoneInfo.Local;
        var localDataReferenceDay = TimeZoneInfo.ConvertTimeFromUtc(dateHelperService.DataReferenceDate, localTimeZone);

        PrintSectionHeader("Cost Analysis");
        Console.WriteLine($"All costs in USD");
        Console.WriteLine($"A day's data is considered complete {DateHelperService.FULL_DAY_DATA_CUTOFF_HOUR_UTC} hours after the end of the day in UTC time.");

        PrintSectionHeader("Monthly Cost Averages");
        PrintCostAverageComparison(currentMonthCostData.Count, averageCurrentPartialMonth, averagePreviousPartialMonth, currentToPreviousMonthAveragesCostDelta);
        Console.WriteLine($"Previous Full Month Average: {averagePreviousFullMonth,10:F2}");

        PrintSectionHeader("Data Reference Information");
        PrintDataReferenceDetails(dateHelperService.DataReferenceDate, localDataReferenceDay, localTimeZone);
    }

    private static void PrintSectionHeader(string headerText)
    {
        Console.WriteLine($"\n------ {headerText} ------");
    }

    private static void PrintCostAverageComparison(int dayCount, decimal currentAverage, decimal previousAverage, decimal costDelta)
    {
        // Using alignment to keep numbers lined up
        Console.WriteLine($"Current month average (for {dayCount,2} days): {currentAverage,10:F2}");
        Console.WriteLine($"Previous month average for same period ({dayCount,2} days): {previousAverage,10:F2}");
        Console.WriteLine($"Current to previous month averages cost delta: {costDelta,10:F2}");
    }

    private static void PrintDataReferenceDetails(DateTime dataReferenceDate, DateTime localDataReferenceDay, TimeZoneInfo localTimeZone)
    {
        Console.WriteLine($"Daily cost data is complete up to end of the day {dataReferenceDate:yyyy-MM-dd} in UTC timezone");
        Console.WriteLine($"The end of the day in UTC time is {localDataReferenceDay} in local timezone of {localTimeZone.DisplayName}");

        Console.WriteLine($"\nThis report was generated at {DateTime.Now} {localTimeZone.DisplayName}");
        Console.WriteLine($"This report was generated at {DateTime.UtcNow} UTC");
    }
}