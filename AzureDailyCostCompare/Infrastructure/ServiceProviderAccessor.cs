using Microsoft.Extensions.DependencyInjection;

namespace AzureDailyCostCompare.Infrastructure;

public static class ServiceProviderAccessor
{
    public static ServiceProvider? ServiceProvider { get; set; }
}