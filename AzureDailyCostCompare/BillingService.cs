using System.Net.Http.Headers;
using System.Text.Json;

namespace AzureDailyCostCompare;

public class BillingService : IBillingService
{
    private static readonly HttpClient _httpClient = new();
    private const string AZURE_BILLING_ACCOUNT_API_BASE_URI = "https://management.azure.com/providers/Microsoft.Billing/billingAccounts?api-version=";
    private const string AZURE_BILLING_ACCOUNT_API_VERSION = "2024-04-01"; // Check for newer API version https://learn.microsoft.com/en-us/rest/api/billing/billing-accounts/get
    private const string AZURE_BILLING_ACCOUNT_API_URI = AZURE_BILLING_ACCOUNT_API_BASE_URI + AZURE_BILLING_ACCOUNT_API_VERSION;

    public async Task<string> GetBillingAccountIdAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestUri = AZURE_BILLING_ACCOUNT_API_URI;
        var response = await _httpClient.GetAsync(requestUri);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            foreach (var item in jsonDocument.RootElement.GetProperty("value").EnumerateArray())
            {
                var firstBillingIdFound = item.GetProperty("id").GetString();
                return firstBillingIdFound!; 
            }
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve billing account: {response.ReasonPhrase}");
        }

        throw new InvalidOperationException("Failed to extract billing account ID from JSON response");
    }
}