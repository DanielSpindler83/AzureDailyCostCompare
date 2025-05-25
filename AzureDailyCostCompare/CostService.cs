using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzureDailyCostCompare;

public class CostService : ICostService
{
    private readonly HttpClient _httpClient;

    // API-specific constants stay in the service
    private const string COST_MANAGEMENT_PATH = "/providers/Microsoft.CostManagement/query";
    private const string API_VERSION = "2023-11-01";

    // Constructor injection of HttpClient
    public CostService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<List<DailyCostData>> QueryCostManagementAPI(string accessToken, string billingAccountId, DateTime startDate, DateTime endDate)
    {
        // Set authorization header for this request
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Build request body
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

        // Build the request URI using the injected base address
        var requestUri = $"{billingAccountId}{COST_MANAGEMENT_PATH}?api-version={API_VERSION}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, requestBody);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return ParseCostData(content);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to query cost management API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {errorContent}");
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to serialize request or parse response JSON", ex);
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions as-is
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error while querying cost management API", ex);
        }
        finally
        {
            // Clear the authorization header to avoid it being used in subsequent requests
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private static List<DailyCostData> ParseCostData(string responseBody)
    {
        try
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
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse cost data from API response", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid date format in API response", ex);
        }
    }
}