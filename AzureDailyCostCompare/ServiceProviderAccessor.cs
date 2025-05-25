using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare;

public static class ServiceProviderAccessor
{
    public static ServiceProvider? ServiceProvider { get; set; }
}