using Azure.Core;
using Azure.Identity;
using AzureDailyCostCompare.Infrastructure.Interfaces;

namespace AzureDailyCostCompare.Infrastructure;

public class AuthenticationService : IAuthenticationService
{
    internal static readonly string[] scopes = ["https://management.azure.com/.default"];

    public async Task<string> GetAccessToken()
    {
        var credential = new AzureCliCredential();
        var tokenRequestContext = new TokenRequestContext(scopes);
        var accessToken = await credential.GetTokenAsync(tokenRequestContext);
        return accessToken.Token;
    }
}