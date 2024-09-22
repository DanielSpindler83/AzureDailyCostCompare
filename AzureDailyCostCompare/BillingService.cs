using System.Net.Http.Headers;
using System.Text.Json;

namespace AzureDailyCostCompare;

class BillingService
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<string> GetBillingAccountIdAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestUri = "https://management.azure.com/providers/Microsoft.Billing/billingAccounts?api-version=2020-05-01";
        var response = await _httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            foreach (var item in jsonDocument.RootElement.GetProperty("value").EnumerateArray())
            {
                var id = item.GetProperty("id").GetString();
                return id; // Return the first billing account ID
            }
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve billing account: {response.ReasonPhrase}");
        }

        throw new InvalidOperationException("Failed to extract billing account ID from JSON response");
    }
}