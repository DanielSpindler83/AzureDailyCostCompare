using AzureDailyCostCompare.Infrastructure.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AzureDailyCostCompare.Infrastructure;

public class BillingService : IBillingService
{
    private readonly HttpClient _httpClient;

    // SPECIFIC API paths and versions stay in the service
    private const string BILLING_ACCOUNTS_PATH = "/providers/Microsoft.Billing/billingAccounts";
    private const string API_VERSION = "2024-04-01";

    public BillingService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> GetBillingAccountIdAsync(string token)
    {
        // Build the full request URI using base address + specific path
        var requestUri = $"{BILLING_ACCOUNTS_PATH}?api-version={API_VERSION}";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);

                foreach (var item in jsonDocument.RootElement.GetProperty("value").EnumerateArray())
                {
                    var firstBillingIdFound = item.GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(firstBillingIdFound))
                    {
                        return firstBillingIdFound;
                    }
                }

                throw new InvalidOperationException("No billing account ID found in response");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to retrieve billing account. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Content: {errorContent}");
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse JSON response from billing API", ex);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error while retrieving billing account", ex);
        }
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}