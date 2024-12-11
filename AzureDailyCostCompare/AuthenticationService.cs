using Azure.Core;
using Azure.Identity;

namespace AzureDailyCostCompare;

static class AuthenticationService
{
    internal static readonly string[] scopes = ["https://management.azure.com/.default"];

    public static async Task<string> GetAccessToken()
    {
        var credential = new AzureCliCredential();
        var tokenRequestContext = new TokenRequestContext(scopes);
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);
        return accessToken.Token;
    }
}