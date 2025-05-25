using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzureDailyCostCompare;

class CostService : ICostService
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<DailyCostData>> QueryCostManagementAPI(string accessToken, string billingAccountId, DateTime startDate, DateTime endDate)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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

        var url = $"https://management.azure.com{billingAccountId}/providers/Microsoft.CostManagement/query?api-version=2023-11-01"; //magic string  - need to pull out and put up top - get clean
        var response = await _httpClient.PostAsJsonAsync(url, requestBody);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return ParseCostData(content);
        }
        else
        {
            throw new HttpRequestException($"Error: {response.ReasonPhrase}");
        }
    }

    private static List<DailyCostData> ParseCostData(string responseBody)
    {
        var jsonDocument = JsonDocument.Parse(responseBody);
        var dailyCostsList = new List<DailyCostData>();

        foreach (var item in jsonDocument.RootElement.GetProperty("properties").GetProperty("rows").EnumerateArray())
        {
            var cost = item[0].GetDecimal();
            var dateNumber = item[1].GetInt64();

            var dailyCost = new DailyCostData
            {
                DateString = DateOnly.FromDateTime(DateTime.ParseExact(dateNumber.ToString(), "yyyyMMdd", null)),
                Cost = Math.Round(cost, 2)
            };

            dailyCostsList.Add(dailyCost);
        }

        return dailyCostsList;
    }
}