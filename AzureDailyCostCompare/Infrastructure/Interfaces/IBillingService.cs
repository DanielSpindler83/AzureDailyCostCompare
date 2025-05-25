// Business logic interfaces and implementations

namespace AzureDailyCostCompare.Infrastructure.Interfaces;

public interface IBillingService
{
    Task<string> GetBillingAccountIdAsync(string accessToken);
}
