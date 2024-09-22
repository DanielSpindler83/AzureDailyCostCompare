
namespace AzureDailyCostCompare;

class ReportGenerator
{
    private List<DailyCosts> costData;
    private DateHelperService dateHelperService;
    private List<DailyCosts> currentMonthCostData;
    private List<DailyCosts> previousMonthCostData;
    private decimal averageCurrentMonth;
    private decimal averageLastMonth;


    public ReportGenerator(List<DailyCosts> costData, DateHelperService dateHelperService)
    {
        this.costData = costData;
        this.dateHelperService = dateHelperService;

        currentMonthCostData = costData
                .Where(dc => dc.DateString.Month == dateHelperService.FirstDayOfCurrentMonth.Month && dc.DateString.Year == dateHelperService.FirstDayOfCurrentMonth.Year)
                .ToList();
        previousMonthCostData = costData
                .Where(dc => dc.DateString.Month == dateHelperService.FirstDayOfPreviousMonth.Month && dc.DateString.Year == dateHelperService.FirstDayOfPreviousMonth.Year)
                .ToList();

        averageCurrentMonth = currentMonthCostData // dont include the last day as the data is not complete and skews the average down
            .Take(currentMonthCostData.Count - 1)
            .Average(dc => dc.Cost);
        averageLastMonth = previousMonthCostData.Average(dc => dc.Cost);

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


        Console.WriteLine("\nDaily Averages in USD:");
        Console.WriteLine("Current Month Average(not inlcuding current day as data is incomplete): {0:F2}", averageCurrentMonth);
        Console.WriteLine("Last Month Average: {0:F2}", averageLastMonth);
    }
}