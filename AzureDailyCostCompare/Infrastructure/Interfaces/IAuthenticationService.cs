// Business logic interfaces and implementations

namespace AzureDailyCostCompare.Infrastructure.Interfaces;

public interface IAuthenticationService
{
    Task<string> GetAccessToken();
}
