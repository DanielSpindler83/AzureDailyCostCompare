using Azure.Core;
using Azure.Identity;

namespace AzureDailyCostCompare;

class AuthenticationService
{
    public async Task<string> GetAccessToken()
    {
        var credential = new AzureCliCredential();
        var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);
        return accessToken.Token;
    }
}