// Business logic interfaces and implementations

namespace AzureDailyCostCompare;

public interface ICostComparisonHandler
{
    Task RunAsync(DateTime? date, bool showWeeklyPatterns, bool showDayOfWeekAverages, int? previousDayUtcDataLoadDelayHours);
}

public interface IAuthenticationService
{
    Task<string> GetAccessToken();
}

public interface IBillingService
{
    Task<string> GetBillingAccountIdAsync(string accessToken);
}

public interface ICostService
{
    Task<List<DailyCostData>> QueryCostManagementAPI(string accessToken, string billingAccountId, DateTime startDate, DateTime endDate);
}