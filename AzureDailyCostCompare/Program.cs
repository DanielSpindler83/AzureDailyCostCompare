namespace AzureDailyCostCompare;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Core;
using System.Net.Http.Json;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Replace with your billing account ID
        // this is NOT a real billing account ID - but its the same format - need to split out to config and explain how to get this
        string billingScope = "/providers/Microsoft.Billing/billingAccounts/a1b2c3d4-e5f6-7890-1234-abcd5678efgh:0987zyxw-4321-vbnm-lkji-8765qwer4321_2019-05-31";

        try
        {
            // you need to be already logged into Azure CLI with an account that has billing account read access
            string accessToken = await GetAccessToken();

            // I believe billing is done via UTC time
            DateTime today = DateTime.UtcNow;

            // if first of month we won't have any billing data for the current month - let's pretend it is last day of previous month
            if (today.Day == 1)
            {
                today = today.AddDays(-1);
            }

            DateTime firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            DateTime firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);

            // Fetch the cost data for the entire date range once
            List<DailyCosts> costData = await QueryCostManagementAPI(accessToken, billingScope, firstDayOfPreviousMonth, today);

            // Filter data for current month and previous month
            var currentMonthData = costData
                .Where(dc => dc.DateString.Month == firstDayOfCurrentMonth.Month && dc.DateString.Year == firstDayOfCurrentMonth.Year)
                .ToList();
            var previousMonthData = costData
                .Where(dc => dc.DateString.Month == firstDayOfPreviousMonth.Month && dc.DateString.Year == firstDayOfPreviousMonth.Year)
                .ToList();

            int daysInPreviousMonth = DateTime.DaysInMonth(firstDayOfPreviousMonth.Year, firstDayOfPreviousMonth.Month);
            int daysInCurrentMonth = DateTime.DaysInMonth(firstDayOfCurrentMonth.Year, firstDayOfCurrentMonth.Month);

            var tableData = from day in Enumerable.Range(1, Math.Max(daysInPreviousMonth, daysInCurrentMonth))
                            join currentDay in currentMonthData on day equals currentDay.DateString.Day into currentDays
                            from currentDay in currentDays.DefaultIfEmpty()
                            join previousDay in previousMonthData on day equals previousDay.DateString.Day into previousDays
                            from previousDay in previousDays.DefaultIfEmpty()
                            select new
                            {
                                DayOfMonth = day,
                                PreviousCost = previousDay?.Cost ?? (day > daysInPreviousMonth ? (decimal?)null : 0),
                                CurrentCost = currentDay?.Cost ?? (day > daysInCurrentMonth ? (decimal?)null : 0),
                                CostDifference = (currentDay?.Cost ?? 0) - (previousDay?.Cost ?? 0)
                            };

            // Print table header
            Console.WriteLine("{0,-18} {1,-18} {2,-18} {3,-18}", "Day of Month", firstDayOfPreviousMonth.ToString("MMMM"), firstDayOfCurrentMonth.ToString("MMMM"), "Cost Difference(USD)");

            // Print table data
            foreach (var row in tableData)
            {
                string previousCost = row.PreviousCost.HasValue ? row.PreviousCost.Value.ToString("F2") : "";
                string currentCost = row.CurrentCost.HasValue ? row.CurrentCost.Value.ToString("F2") : "";
                string costDifference = row.CostDifference.ToString("F2");

                Console.WriteLine("{0,-18} {1,-18} {2,-18} {3,-18}", row.DayOfMonth, previousCost, currentCost, costDifference);
            }

            // Calculate daily averages using LINQ
            //var averageCurrentMonth = currentMonthData.Average(dc => dc.Cost);
            var averageCurrentMonth = currentMonthData // dont include the last day as the data is not complete and skews the average down
                .Take(currentMonthData.Count - 1)
                .Average(dc => dc.Cost);
            var averageLastMonth = previousMonthData.Average(dc => dc.Cost);

            Console.WriteLine("\nDaily Averages in USD:");
            Console.WriteLine("Current Month Average(not inlcuding current day as data is incomplete): {0:F2}", averageCurrentMonth);
            Console.WriteLine("Last Month Average: {0:F2}", averageLastMonth);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static DateTime GetValidPreviousMonthDate(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day).AddMonths(-1);
    }

    private static async Task<string> GetAccessToken()
    {
        // Authenticate using Azure CLI
        var credential = new AzureCliCredential();
        var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);
        return accessToken.Token;
    }

    private static async Task<List<DailyCosts>> QueryCostManagementAPI(string accessToken, string billingScope, DateTime startDate, DateTime endDate)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Cost Management API URL
            string url = $"https://management.azure.com{billingScope}/providers/Microsoft.CostManagement/query?api-version=2023-11-01";
            // https://learn.microsoft.com/en-us/rest/api/cost-management/query/usage?view=rest-cost-management-2023-11-01&tabs=HTTP

            // Define request body
            var requestBody = new
            {
                type = "ActualCost",
                timeframe = "Custom",
                timePeriod = new
                {
                    from = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    to = endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
                },
                dataSet = new
                {
                    granularity = "Daily",
                    aggregation = new
                    {
                        totalCostUSD = new
                        {
                            name = "CostUSD",
                            function = "Sum"
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(url, requestBody);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ParseCostData(responseBody);
            }
            else
            {
                throw new HttpRequestException($"Error: {responseBody}");
            }
        }
    }


    private static List<DailyCosts> ParseCostData(string responseBody)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonDocument = JsonDocument.Parse(responseBody);
        var dailyCostsList = new List<DailyCosts>();

        foreach (var item in jsonDocument.RootElement.GetProperty("properties").GetProperty("rows").EnumerateArray())
        {
            var cost = item[0].GetDecimal();
            var dateNumber = item[1].GetInt64();

            var dailyCost = new DailyCosts
            {
                DateString = DateOnly.FromDateTime(DateTime.ParseExact(dateNumber.ToString(), "yyyyMMdd", null)),
                Cost = Math.Round(cost, 2)
            };

            dailyCostsList.Add(dailyCost);
        }

        return dailyCostsList;
    }

}
