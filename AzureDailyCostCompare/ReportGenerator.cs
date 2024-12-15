namespace AzureDailyCostCompare;

class ReportGenerator
{
    private readonly DateHelperService dateHelperService;
    private readonly List<DailyCosts> currentMonthCostData;
    private readonly List<DailyCosts> previousMonthCostData;
    private readonly decimal averageCurrentPartialMonth;
    private readonly decimal averagePreviousPartialMonth;
    private readonly decimal averagePreviousFullMonth;


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


        /// do we move this to datehelperservice as a static method maybe? Idea is keep all date\time logic in one palce
        // not ideal but the time created isnt really tied to when we grabbed the datafrom the api - just when we instantiated the dateHelperService - something to be weary of if making changes
        //DateTime localDateTimeToday = TimeZoneInfo.ConvertTimeFromUtc(dateHelperService.DataReferenceDate, TimeZoneInfo.Local);

        // still want to show the time that we displayed the results (in users timezone)

        Console.WriteLine("\nALL costs in USD and for full day periods only - no partial day cost data is displayed.");
        Console.WriteLine("Current Month Average(for {0} days) : {1:F2}", currentMonthCostData.Count, averageCurrentPartialMonth);
        Console.WriteLine("Previous Month Average for same period({0} days) : {1:F2}", currentMonthCostData.Count, averagePreviousPartialMonth);
        Console.WriteLine("Rolling averages only include full days.");
        Console.WriteLine("------");
        Console.WriteLine("Previous Full Month Average: {0:F2}", averagePreviousFullMonth);
        Console.WriteLine("------");
        // Console.WriteLine("Time data was retrieved from Microsoft Azure Cost Management API(approx. & in local system timezone): {0}", localDateTimeToday);
        Console.WriteLine(dateHelperService.GetDataCurrencyDescription());
        Console.WriteLine("------\n");
    }
}