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

        Console.WriteLine("\nAll costs in USD");
        Console.WriteLine("A day's data is considered complete {0} hours after the end of the day in UTC time.", DateHelperService.FULL_DAY_DATA_CUTOFF_HOUR);
        Console.WriteLine("------");
        Console.WriteLine("Current month average(for {0} days) : {1:F2}", currentMonthCostData.Count, averageCurrentPartialMonth);
        Console.WriteLine("Previous month average for same period({0} days) : {1:F2}", currentMonthCostData.Count, averagePreviousPartialMonth);
        Console.WriteLine("Current to previous month averages cost delta : {0:F2}", currentToPreviousMonthAveragesCostDelta);
        Console.WriteLine("------");
        Console.WriteLine("Previous Full Month Average: {0:F2}", averagePreviousFullMonth);
        Console.WriteLine("------");
        Console.WriteLine(dateHelperService.GetDataCurrencyDescription());
    }
}