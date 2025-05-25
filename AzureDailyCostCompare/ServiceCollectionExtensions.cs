using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace AzureDailyCostCompare;

public static class ServiceCollectionExtensions
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.ConfigureServices();

        return services.BuildServiceProvider();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register HTTP client
        services.AddHttpClient();

        // Register your business services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<ICostService, CostService>();
        services.AddScoped<ICostComparisonHandler, CostComparisonBusinessHandler>();

        // If you need specific HTTP client configurations:
        services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
        {
            client.BaseAddress = new Uri("https://login.microsoftonline.com/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        services.AddHttpClient<IBillingService, BillingService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddHttpClient<ICostService, CostService>(client =>
        {
            client.BaseAddress = new Uri("https://management.azure.com/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        return services;
    }
}