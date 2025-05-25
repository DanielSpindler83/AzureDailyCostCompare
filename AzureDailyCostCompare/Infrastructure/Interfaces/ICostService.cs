using AzureDailyCostCompare.Application;

namespace AzureDailyCostCompare.Infrastructure.Interfaces;

public interface ICostService
{
    Task<List<DailyCostData>> QueryCostManagementAPI(string accessToken, string billingAccountId, DateTime startDate, DateTime endDate);
}