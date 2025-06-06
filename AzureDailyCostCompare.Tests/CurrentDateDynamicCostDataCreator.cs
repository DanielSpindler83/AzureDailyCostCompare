using AzureDailyCostCompare.Domain;

namespace AzureDailyCostCompare.Tests;

public class CurrentDateDynamicCostDataCreator
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Generates mock cost data for the current month and previous month
    /// </summary>
    /// <param name="baseCost">Base cost to fluctuate around (default: 150.0)</param>
    /// <param name="variationPercent">Percentage variation for cost fluctuation (default: 30%)</param>
    /// <returns>List of DailyCostData objects</returns>
    public static List<DailyCostData> GenerateCostData(decimal baseCost = 150.0m, double variationPercent = 30.0)
    {
        var costData = new List<DailyCostData>();
        var today = DateTime.Today;

        // Generate data for previous month
        var previousMonth = today.AddMonths(-1);
        var previousMonthData = GenerateMonthData(previousMonth, baseCost, variationPercent);
        costData.AddRange(previousMonthData);

        // Generate data for current month (up to today)
        var currentMonthData = GenerateMonthData(today, baseCost, variationPercent, upToToday: true);
        costData.AddRange(currentMonthData);

        return costData.OrderBy(x => x.DateString).ToList();
    }

    /// <summary>
    /// Generates mock cost data for a specific month
    /// </summary>
    /// <param name="month">The month to generate data for</param>
    /// <param name="baseCost">Base cost to fluctuate around</param>
    /// <param name="variationPercent">Percentage variation for cost fluctuation</param>
    /// <param name="upToToday">If true, only generate data up to today's date</param>
    /// <returns>List of DailyCostData for the specified month</returns>
    public static List<DailyCostData> GenerateMonthData(DateTime month, decimal baseCost = 150.0m, double variationPercent = 30.0, bool upToToday = false)
    {
        var costData = new List<DailyCostData>();
        var firstDay = new DateTime(month.Year, month.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        // If upToToday is true and we're in the current month, limit to today
        if (upToToday && month.Month == DateTime.Today.Month && month.Year == DateTime.Today.Year)
        {
            lastDay = DateTime.Today;
        }

        var currentDate = firstDay;
        var trendFactor = 1.0; // Factor to create gradual trend over the month

        while (currentDate <= lastDay)
        {
            // Create a gradual upward trend throughout the month
            var trendMultiplier = 1.0m + (decimal)(trendFactor * 0.1); // Up to 10% increase by end of month

            // Add random daily variation
            var variation = (decimal)(variationPercent / 100.0) * baseCost;
            var randomVariation = (decimal)((_random.NextDouble() - 0.5) * 2) * variation; // -variation to +variation

            var dailyCost = Math.Round((baseCost * trendMultiplier) + randomVariation, 2);

            // Ensure cost is always positive
            dailyCost = Math.Max(dailyCost, 10.0m);

            costData.Add(new DailyCostData
            {
                DateString = DateOnly.FromDateTime(currentDate),
                Cost = dailyCost
            });

            currentDate = currentDate.AddDays(1);
            trendFactor += 0.03; // Gradual trend increase
        }

        return costData;
    }

    /// <summary>
    /// Generates cost data for a specific date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="baseCost">Base cost to fluctuate around</param>
    /// <param name="variationPercent">Percentage variation for cost fluctuation</param>
    /// <returns>List of DailyCostData for the specified date range</returns>
    public static List<DailyCostData> GenerateDateRangeData(DateTime startDate, DateTime endDate, decimal baseCost = 150.0m, double variationPercent = 30.0)
    {
        var costData = new List<DailyCostData>();
        var currentDate = startDate.Date;
        var totalDays = (endDate.Date - startDate.Date).TotalDays + 1;
        var dayCounter = 0;

        while (currentDate <= endDate.Date)
        {
            // Create a gradual trend over the entire date range
            var trendMultiplier = 1.0m + (decimal)(dayCounter / totalDays * 0.2); // Up to 20% increase over the range

            // Add random daily variation
            var variation = (decimal)(variationPercent / 100.0) * baseCost;
            var randomVariation = (decimal)((_random.NextDouble() - 0.5) * 2) * variation;

            var dailyCost = Math.Round((baseCost * trendMultiplier) + randomVariation, 2);
            dailyCost = Math.Max(dailyCost, 10.0m);

            costData.Add(new DailyCostData
            {
                DateString = DateOnly.FromDateTime(currentDate),
                Cost = dailyCost
            });

            currentDate = currentDate.AddDays(1);
            dayCounter++;
        }

        return costData;
    }
}